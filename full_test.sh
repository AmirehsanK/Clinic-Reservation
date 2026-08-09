#!/bin/bash
# End-to-end suite driven through the real HTTP surface: every controller action
# is exercised against a freshly seeded database. Run it via run-tests.ps1, which
# drops the database and starts the app first.
BASE="http://localhost:5299"
JAR=/tmp/cookies_full.txt
LOG=/tmp/app_full.log
PASS=0
FAIL=0

rm -f $JAR

# Antiforgery tokens are rendered both inside POST forms and once per page by
# _Layout; any of them pairs with the same session cookie.
get_token() {
  grep -o '__RequestVerificationToken[^>]*value="[^"]*"' "$1" | head -1 | sed 's/.*value="//;s/"$//'
}

# Fetches a page purely to harvest a fresh antiforgery token.
token_from() {
  curl -s -c $JAR -b $JAR "$BASE$1" -o /tmp/_tok.html
  get_token /tmp/_tok.html
}

check() {
  # check "description" actual expected
  if [ "$2" == "$3" ]; then
    echo "PASS: $1 ($2)"
    PASS=$((PASS+1))
  else
    echo "FAIL: $1 (got '$2', expected '$3')"
    FAIL=$((FAIL+1))
  fi
}

check_contains() {
  # check_contains "description" file needle
  if grep -q "$3" "$2" 2>/dev/null; then
    echo "PASS: $1"
    PASS=$((PASS+1))
  else
    echo "FAIL: $1 (needle not found: $3 in $2)"
    FAIL=$((FAIL+1))
  fi
}

check_absent() {
  # check_absent "description" file needle
  if grep -q "$3" "$2" 2>/dev/null; then
    echo "FAIL: $1 (unexpectedly found: $3 in $2)"
    FAIL=$((FAIL+1))
  else
    echo "PASS: $1"
    PASS=$((PASS+1))
  fi
}

check_contains_str() {
  if echo "$2" | grep -q "$3"; then
    echo "PASS: $1"; PASS=$((PASS+1))
  else
    echo "FAIL: $1 -> $2"; FAIL=$((FAIL+1))
  fi
}

# Deletes are POST now, so they need a token. Echoes the HTTP status.
post_delete() {
  # post_delete <path> <page-to-take-token-from> <output-file>
  local T
  T=$(token_from "$2")
  curl -s -c $JAR -b $JAR -X POST "$BASE$1" \
    --data-urlencode "__RequestVerificationToken=$T" \
    -o "$3" -w "%{http_code}"
}

echo "############ AUTH: FAILURE PATHS ############"
CODE=$(curl -s "$BASE/Patient/FilterPatients" -o /dev/null -w "%{http_code}")
check "Anonymous access redirects to login" "$CODE" "302"

curl -s -c $JAR "$BASE/Account/Login" -o /tmp/t_login0.html
TOKEN=$(get_token /tmp/t_login0.html)
CODE=$(curl -s -c $JAR -b $JAR -X POST "$BASE/Account/Login" \
  --data-urlencode "Mobile=09999999999" --data-urlencode "__RequestVerificationToken=$TOKEN" \
  -o /tmp/t_login_bad.html -w "%{http_code}")
check "Login with unknown mobile stays on form" "$CODE" "200"
check_contains "Unknown mobile shows error" /tmp/t_login_bad.html "not found"

echo "############ AUTH: LOGIN ############"
rm -f $JAR
curl -s -c $JAR "$BASE/Account/Login" -o /tmp/t_login1.html
TOKEN=$(get_token /tmp/t_login1.html)
CODE=$(curl -s -c $JAR -b $JAR -X POST "$BASE/Account/Login" \
  --data-urlencode "Mobile=09120000000" --data-urlencode "__RequestVerificationToken=$TOKEN" \
  -o /tmp/t_login2.html -w "%{http_code}")
check "Login POST redirects" "$CODE" "302"
sleep 1

# The stub SMS gateway logs the bare code; anything else means Login is passing a
# formatted message where the code belongs.
OTP_LINE=$(grep "OTP for 09120000000" $LOG | tail -1)
OTP=$(echo "$OTP_LINE" | sed 's/.*: //' | tr -d '\r')
if echo "$OTP" | grep -qE '^[0-9]{6}$'; then
  echo "PASS: SMS gateway receives the bare 6-digit code"
  PASS=$((PASS+1))
else
  echo "FAIL: SMS gateway receives the bare 6-digit code (got '$OTP')"
  FAIL=$((FAIL+1))
fi

curl -s -c $JAR -b $JAR "$BASE/Account/Authentication?mobile=09120000000" -o /tmp/t_auth0.html
TOKEN=$(get_token /tmp/t_auth0.html)
CODE=$(curl -s -c $JAR -b $JAR -X POST "$BASE/Account/Authentication" \
  --data-urlencode "Mobile=09120000000" --data-urlencode "OtpCode=000000" --data-urlencode "__RequestVerificationToken=$TOKEN" \
  -o /tmp/t_auth_bad.html -w "%{http_code}")
check "Wrong OTP redirects back to login" "$CODE" "302"

curl -s -c $JAR -b $JAR "$BASE/Account/Authentication?mobile=09120000000" -o /tmp/t_auth1.html
TOKEN=$(get_token /tmp/t_auth1.html)
CODE=$(curl -s -c $JAR -b $JAR -X POST "$BASE/Account/Authentication" \
  --data-urlencode "Mobile=09120000000" --data-urlencode "OtpCode=$OTP" --data-urlencode "__RequestVerificationToken=$TOKEN" \
  -o /tmp/t_auth2.html -w "%{http_code}")
check "OTP POST redirects to dashboard" "$CODE" "302"

curl -s -b $JAR "$BASE/Home/Dashboard" -o /tmp/t_dash.html -w ""
check_contains "Dashboard renders for signed-in admin" /tmp/t_dash.html "Clinic Reservation"

echo "############ SECURITY: CSRF ############"
CODE=$(curl -s -b $JAR -X POST "$BASE/delete-patient/2" -o /dev/null -w "%{http_code}")
check "POST delete without antiforgery token is rejected" "$CODE" "400"
CODE=$(curl -s -b $JAR "$BASE/delete-patient/2" -o /dev/null -w "%{http_code}")
check "GET on a delete route is refused (405)" "$CODE" "405"
curl -s -b $JAR "$BASE/Patient/FilterPatients?NationalId=1000000002" -o /tmp/t_csrf.html
check_contains "Patient survived the rejected CSRF delete" /tmp/t_csrf.html "1000000002"

echo "############ PATIENT: READ / FILTER ############"
curl -s -b $JAR "$BASE/Patient/FilterPatients" -o /tmp/t_p_list1.html
check_contains "Patient list shows first page results" /tmp/t_p_list1.html "Michael Johnson"
curl -s -b $JAR "$BASE/Patient/FilterPatients?Gender=1&TakeEntity=100" -o /tmp/t_p_male.html
check_contains "Filter Gender=Male includes a male patient" /tmp/t_p_male.html "1000000001"
check_absent "Filter Gender=Male excludes a female patient" /tmp/t_p_male.html "1000000002"
curl -s -b $JAR "$BASE/Patient/FilterPatients?Gender=2&TakeEntity=100" -o /tmp/t_p_female.html
check_contains "Filter Gender=Female includes a female patient" /tmp/t_p_female.html "1000000002"
check_absent "Filter Gender=Female excludes a male patient" /tmp/t_p_female.html "1000000001"
curl -s -b $JAR "$BASE/Patient/FilterPatients?Gender=3&TakeEntity=100" -o /tmp/t_p_ns.html
check_contains "Filter Gender=NotSpecified works" /tmp/t_p_ns.html "1000000019"
curl -s -b $JAR "$BASE/Patient/FilterPatients?FullName=Sarah" -o /tmp/t_p_name.html
check_contains "Filter by FullName finds Sarah Williams" /tmp/t_p_name.html "Sarah Williams"
curl -s -b $JAR "$BASE/Patient/FilterPatients?NationalId=1000000005" -o /tmp/t_p_nid.html
check_contains "Filter by NationalId finds James Miller" /tmp/t_p_nid.html "James Miller"
curl -s -b $JAR "$BASE/Patient/FilterPatients?Mobile=09121110007" -o /tmp/t_p_mob.html
check_contains "Filter by Mobile finds Robert Moore" /tmp/t_p_mob.html "Robert Moore"
curl -s -b $JAR "$BASE/Patient/FilterPatients?Description=penicillin" -o /tmp/t_p_desc.html
check_contains "Filter by Description finds the right patient" /tmp/t_p_desc.html "1000000007"
curl -s -b $JAR "$BASE/Patient/FilterPatients?Age=52" -o /tmp/t_p_age.html
check_contains "Filter by Age finds James Miller" /tmp/t_p_age.html "1000000005"
curl -s -b $JAR "$BASE/Patient/FilterPatients?PageId=2" -o /tmp/t_p_page2.html
check_contains "Pagination page 2 shows different patient" /tmp/t_p_page2.html "Samantha Lewis"

echo "############ PATIENT: CREATE ############"
TOKEN=$(token_from "/create-patient")
CODE=$(curl -s -c $JAR -b $JAR -X POST "$BASE/create-patient" \
  --data-urlencode "FullName=Test Patient Alpha" --data-urlencode "Mobile=09129990001" \
  --data-urlencode "NationalId=9000000001" --data-urlencode "Age=30" --data-urlencode "Gender=Female" \
  --data-urlencode "Description=Created via test" --data-urlencode "__RequestVerificationToken=$TOKEN" \
  -o /tmp/t_cp2.html -w "%{http_code}")
check "Create patient succeeds" "$CODE" "302"

# Gender and Description used to be dropped on the way into the database.
curl -s -b $JAR "$BASE/Patient/FilterPatients?Mobile=09129990001&Gender=2&TakeEntity=100" -o /tmp/t_cp_gender.html
check_contains "Created patient kept Gender=Female" /tmp/t_cp_gender.html "9000000001"
curl -s -b $JAR "$BASE/Patient/FilterPatients?Description=Created%20via%20test&TakeEntity=100" -o /tmp/t_cp_desc.html
check_contains "Created patient kept its Description" /tmp/t_cp_desc.html "9000000001"

TOKEN=$(token_from "/create-patient")
CODE=$(curl -s -c $JAR -b $JAR -X POST "$BASE/create-patient" \
  --data-urlencode "FullName=Duplicate Mobile Patient" --data-urlencode "Mobile=09129990001" \
  --data-urlencode "NationalId=9000000099" --data-urlencode "Age=25" --data-urlencode "Gender=Female" \
  --data-urlencode "__RequestVerificationToken=$TOKEN" \
  -o /tmp/t_cp4.html -w "%{http_code}")
check "Duplicate mobile create returns form (200)" "$CODE" "200"
check_contains "Duplicate mobile shows error alert" /tmp/t_cp4.html "already exists"

TOKEN=$(token_from "/create-patient")
CODE=$(curl -s -c $JAR -b $JAR -X POST "$BASE/create-patient" \
  --data-urlencode "FullName=Duplicate NID Patient" --data-urlencode "Mobile=09129990077" \
  --data-urlencode "NationalId=9000000001" --data-urlencode "Age=25" --data-urlencode "Gender=Male" \
  --data-urlencode "__RequestVerificationToken=$TOKEN" \
  -o /tmp/t_cp5.html -w "%{http_code}")
check "Duplicate national ID create returns form (200)" "$CODE" "200"
check_contains "Duplicate national ID shows error alert" /tmp/t_cp5.html "national ID"

echo "############ PATIENT: CREATE GROUP ############"
TOKEN=$(token_from "/create-group-patients")
CODE=$(curl -s -c $JAR -b $JAR -X POST "$BASE/create-group-patients" \
  --data-urlencode "[0].FullName=Batch Patient One" --data-urlencode "[0].Mobile=09129990010" \
  --data-urlencode "[0].NationalId=9000000010" --data-urlencode "[0].Age=22" --data-urlencode "[0].Gender=Female" \
  --data-urlencode "[1].FullName=Batch Patient Two" --data-urlencode "[1].Mobile=09129990001" \
  --data-urlencode "[1].NationalId=9000000011" --data-urlencode "[1].Age=27" --data-urlencode "[1].Gender=Male" \
  --data-urlencode "__RequestVerificationToken=$TOKEN" \
  -o /tmp/t_cgp2.html -w "%{http_code}")
check "Batch create (partial duplicate) redirects" "$CODE" "302"
curl -s -b $JAR "$BASE/Patient/FilterPatients?FullName=Batch&TakeEntity=100" -o /tmp/t_cgp3.html
check_contains "Batch valid row was created" /tmp/t_cgp3.html "9000000010"
check_absent "Batch duplicate row was rejected" /tmp/t_cgp3.html "9000000011"

# Two identical rows in one submission: only the first may be accepted.
TOKEN=$(token_from "/create-group-patients")
curl -s -c $JAR -b $JAR -X POST "$BASE/create-group-patients" \
  --data-urlencode "[0].FullName=Twin Row" --data-urlencode "[0].Mobile=09129990020" \
  --data-urlencode "[0].NationalId=9000000020" --data-urlencode "[0].Age=40" --data-urlencode "[0].Gender=Male" \
  --data-urlencode "[1].FullName=Twin Row Copy" --data-urlencode "[1].Mobile=09129990020" \
  --data-urlencode "[1].NationalId=9000000021" --data-urlencode "[1].Age=41" --data-urlencode "[1].Gender=Male" \
  --data-urlencode "__RequestVerificationToken=$TOKEN" -o /dev/null
curl -s -b $JAR "$BASE/Patient/FilterPatients?Mobile=09129990020&TakeEntity=100" -o /tmp/t_twin.html
TWINS=$(grep -c 'edit-patient/[0-9]*"' /tmp/t_twin.html)
check "In-batch duplicate mobile created only one row" "$TWINS" "1"

echo "############ PATIENT: UPDATE ############"
curl -s -b $JAR "$BASE/Patient/FilterPatients?Mobile=09129990001&TakeEntity=100" -o /tmp/t_findid.html
PATIENT_ID=$(grep -o 'edit-patient/[0-9]*' /tmp/t_findid.html | head -1 | sed 's/edit-patient\///')
echo "Test Patient Alpha id=$PATIENT_ID"

curl -s -c $JAR -b $JAR "$BASE/edit-patient/$PATIENT_ID" -o /tmp/t_ep1.html
check_contains "Edit form is prefilled with the existing Description" /tmp/t_ep1.html "Created via test"
TOKEN=$(get_token /tmp/t_ep1.html)
CODE=$(curl -s -c $JAR -b $JAR -X POST "$BASE/edit-patient/$PATIENT_ID" \
  --data-urlencode "Id=$PATIENT_ID" --data-urlencode "FullName=Test Patient Alpha Updated" \
  --data-urlencode "Mobile=09129990001" --data-urlencode "NationalId=9000000001" \
  --data-urlencode "Age=31" --data-urlencode "Gender=Male" --data-urlencode "Description=Updated notes" \
  --data-urlencode "__RequestVerificationToken=$TOKEN" \
  -o /tmp/t_ep2.html -w "%{http_code}")
check "Edit patient succeeds" "$CODE" "302"
curl -s -b $JAR "$BASE/Patient/FilterPatients?FullName=Test%20Patient%20Alpha%20Updated" -o /tmp/t_ep3.html
check_contains "Edit patient reflects new name" /tmp/t_ep3.html "Test Patient Alpha Updated"
curl -s -b $JAR "$BASE/Patient/FilterPatients?Description=Updated%20notes&TakeEntity=100" -o /tmp/t_ep4.html
check_contains "Edit patient persisted the new Description" /tmp/t_ep4.html "9000000001"
curl -s -b $JAR "$BASE/Patient/FilterPatients?Mobile=09129990001&Gender=1&TakeEntity=100" -o /tmp/t_ep5.html
check_contains "Edit patient persisted the new Gender" /tmp/t_ep5.html "9000000001"

CODE=$(curl -s -b $JAR "$BASE/edit-patient/999999" -o /dev/null -w "%{http_code}")
check "Edit form for unknown patient returns 404" "$CODE" "404"

echo "############ PATIENT: DELETE ############"
CODE=$(post_delete "/delete-patient/$PATIENT_ID" "/Patient/FilterPatients" /tmp/t_dp1.html)
check "Delete patient (no records) redirects" "$CODE" "302"
curl -s -b $JAR "$BASE/Patient/FilterPatients?FullName=Test%20Patient%20Alpha&TakeEntity=100" -o /tmp/t_dp2.html
check_absent "Patient removed from list after delete" /tmp/t_dp2.html "edit-patient/$PATIENT_ID\""

# Michael Johnson (seed patient 1) has records, so a plain delete must be refused.
CODE=$(post_delete "/delete-patient/1" "/Patient/FilterPatients" /tmp/t_dp3.html)
check "Delete patient with records still redirects" "$CODE" "302"
curl -s -b $JAR "$BASE/Patient/FilterPatients?NationalId=1000000001" -o /tmp/t_dp4.html
check_contains "Patient with records NOT deleted (protected)" /tmp/t_dp4.html "Michael Johnson"

CODE=$(post_delete "/delete-patient/999999" "/Patient/FilterPatients" /tmp/t_dp5.html)
check "Delete unknown patient degrades gracefully" "$CODE" "302"

echo "############ PATIENT: DELETE WITH RECORDS ############"
curl -s -b $JAR "$BASE/Patient/FilterPatients?NationalId=1000000003&TakeEntity=100" -o /tmp/t_dwr0.html
DWR_ID=$(grep -o 'edit-patient/[0-9]*' /tmp/t_dwr0.html | head -1 | sed 's/edit-patient\///')
echo "David Brown id=$DWR_ID"
CODE=$(post_delete "/delete-patient-with-records/$DWR_ID" "/Patient/FilterPatients" /tmp/t_dwr1.html)
check "Delete patient with all records succeeds" "$CODE" "302"
curl -s -b $JAR "$BASE/Patient/FilterPatients?NationalId=1000000003&TakeEntity=100" -o /tmp/t_dwr2.html
check_absent "Cascade-deleted patient is gone" /tmp/t_dwr2.html "David Brown"
curl -s -b $JAR "$BASE/Record/Index?PatientId=$DWR_ID&TakeEntity=100" -o /tmp/t_dwr3.html
check_absent "Cascade-deleted patient's records are gone" /tmp/t_dwr3.html "edit-record/"

echo "############ RESERVATION: READ / FILTER ############"
curl -s -b $JAR "$BASE/Reservation/FilterReservation" -o /tmp/t_r_list1.html
check_contains "Reservation list shows results" /tmp/t_r_list1.html "status-dot"
curl -s -b $JAR "$BASE/Reservation/FilterReservation?FilterReservationStatus=Reserved" -o /tmp/t_r_reserved.html
check_contains "Filter Reserved shows red dot" /tmp/t_r_reserved.html "bg-red"
check_absent "Filter Reserved excludes free slots" /tmp/t_r_reserved.html "status-dot-animated bg-green"
curl -s -b $JAR "$BASE/Reservation/FilterReservation?FilterReservationStatus=NotReserved" -o /tmp/t_r_free.html
check_contains "Filter NotReserved shows green dot" /tmp/t_r_free.html "bg-green"

# Paging must be stable now that the query is ordered.
curl -s -b $JAR "$BASE/Reservation/FilterReservation?PageId=2&TakeEntity=10" -o /tmp/t_r_pg2a.html
curl -s -b $JAR "$BASE/Reservation/FilterReservation?PageId=2&TakeEntity=10" -o /tmp/t_r_pg2b.html
IDS_A=$(grep -o 'reservation-checkbox" value="[0-9]*"' /tmp/t_r_pg2a.html | grep -o '[0-9]*' | paste -sd, -)
IDS_B=$(grep -o 'reservation-checkbox" value="[0-9]*"' /tmp/t_r_pg2b.html | grep -o '[0-9]*' | paste -sd, -)
check "Paged reservation results are stable across requests" "$IDS_A" "$IDS_B"

echo "############ RESERVATION: CREATE SINGLE ############"
TOKEN=$(token_from "/create-single-reservation")
CODE=$(curl -s -c $JAR -b $JAR -X POST "$BASE/create-single-reservation" \
  --data-urlencode "ReserveTime=2030-06-01T09:00" --data-urlencode "EndReserveTime=2030-06-01T09:30" \
  --data-urlencode "__RequestVerificationToken=$TOKEN" \
  -o /tmp/t_cr2.html -w "%{http_code}")
check "Create reservation succeeds" "$CODE" "302"

TOKEN=$(token_from "/create-single-reservation")
CODE=$(curl -s -c $JAR -b $JAR -X POST "$BASE/create-single-reservation" \
  --data-urlencode "ReserveTime=2030-06-01T09:15" --data-urlencode "EndReserveTime=2030-06-01T09:45" \
  --data-urlencode "__RequestVerificationToken=$TOKEN" \
  -o /tmp/t_cr4.html -w "%{http_code}")
check "Overlapping reservation rejected (200 with error)" "$CODE" "200"
check_contains "Overlap error message shown" /tmp/t_cr4.html "already reserved"

TOKEN=$(token_from "/create-single-reservation")
CODE=$(curl -s -c $JAR -b $JAR -X POST "$BASE/create-single-reservation" \
  --data-urlencode "ReserveTime=2030-06-02T11:00" --data-urlencode "EndReserveTime=2030-06-02T10:00" \
  --data-urlencode "__RequestVerificationToken=$TOKEN" \
  -o /tmp/t_cr6.html -w "%{http_code}")
check "Reservation ending before it starts is rejected" "$CODE" "200"
check_contains "Backwards time range shows an error" /tmp/t_cr6.html "after the start time"

echo "############ RESERVATION: CREATE GROUP ############"
TOKEN=$(token_from "/create-group-reservation")
CODE=$(curl -s -c $JAR -b $JAR -X POST "$BASE/create-group-reservation" \
  --data-urlencode "Year=2030" --data-urlencode "Month=7" \
  --data-urlencode "VisitDays=Monday" --data-urlencode "VisitTimes[0]=13:00" \
  --data-urlencode "VisitDuration=30" --data-urlencode "__RequestVerificationToken=$TOKEN" \
  -o /tmp/t_cgr2.html -w "%{http_code}")
check "Create group reservation succeeds" "$CODE" "302"
curl -s -b $JAR "$BASE/Reservation/FilterReservation?ReserveTime=2030-07-01&TakeEntity=200" -o /tmp/t_cgr3.html
check_contains "Group reservation created Mondays in July 2030" /tmp/t_cgr3.html "Jul 01, 2030"

# Overlapping visit times inside one submission must not both be created.
TOKEN=$(token_from "/create-group-reservation")
curl -s -c $JAR -b $JAR -X POST "$BASE/create-group-reservation" \
  --data-urlencode "Year=2031" --data-urlencode "Month=3" \
  --data-urlencode "VisitDays=Tuesday" \
  --data-urlencode "VisitTimes[0]=10:00" --data-urlencode "VisitTimes[1]=10:15" \
  --data-urlencode "VisitDuration=30" --data-urlencode "__RequestVerificationToken=$TOKEN" \
  -o /tmp/t_cgr4.html
# March 2031 has four Tuesdays; the 10:15 slot overlaps the 10:00 one, so only
# four rows may exist - eight would mean the in-batch check did nothing.
curl -s -b $JAR "$BASE/Reservation/FilterReservation?ReserveTime=2031-03-01&TakeEntity=200" -o /tmp/t_cgr5.html
DUP_SLOTS=$(grep -o 'reservation-checkbox" value="[0-9]*"' /tmp/t_cgr5.html | wc -l | tr -d ' ')
check "Self-overlapping group slots created one row per Tuesday" "$DUP_SLOTS" "4"
check_contains "Group slot kept the first visit time" /tmp/t_cgr5.html "Mar 04, 2031 10:00"
check_absent "Overlapping second visit time was skipped" /tmp/t_cgr5.html "Mar 04, 2031 10:15"

echo "############ RESERVATION: DELETE ############"
curl -s -b $JAR "$BASE/Reservation/FilterReservation?ReserveTime=2030-06-01&TakeEntity=200" -o /tmp/t_find_res.html
RES_ID=$(grep -o 'reservation-checkbox" value="[0-9]*"' /tmp/t_find_res.html | head -1 | grep -o '[0-9]*')
echo "New reservation id=$RES_ID"
CODE=$(post_delete "/delete-reservation/$RES_ID" "/Reservation/FilterReservation" /tmp/t_delr1.html)
check "Delete free reservation succeeds" "$CODE" "302"
curl -s -b $JAR "$BASE/Reservation/FilterReservation?ReserveTime=2030-06-01&TakeEntity=200" -o /tmp/t_delr1b.html
check_absent "Deleted reservation is gone from the list" /tmp/t_delr1b.html "value=\"$RES_ID\""

# Reservation 1 is a seeded past slot that has a record attached.
CODE=$(post_delete "/delete-reservation/1" "/Reservation/FilterReservation" /tmp/t_delr2.html)
check "Delete in-use reservation still redirects" "$CODE" "302"
curl -s -b $JAR "$BASE/Record/Index?ReservationId=1&TakeEntity=100" -o /tmp/t_delr3.html
check_contains "In-use reservation was NOT deleted" /tmp/t_delr3.html "edit-record/"

CODE=$(post_delete "/delete-reservation/999999" "/Reservation/FilterReservation" /tmp/t_delr4.html)
check "Delete unknown reservation degrades gracefully" "$CODE" "302"

echo "############ RESERVATION: DELETE GROUP (AJAX) ############"
curl -s -b $JAR "$BASE/Reservation/FilterReservation?ReserveTime=2030-07-01&TakeEntity=200" -o /tmp/t_find_group.html
IDS=$(grep -o 'reservation-checkbox" value="[0-9]*"' /tmp/t_find_group.html | grep -o '[0-9]*' | head -3 | paste -sd, -)
TOKEN=$(token_from "/Reservation/FilterReservation")
echo "Deleting reservation ids: [$IDS]"
RESPONSE=$(curl -s -c $JAR -b $JAR -X POST "$BASE/delete-group-reservation" \
  -H "Content-Type: application/json" -H "RequestVerificationToken: $TOKEN" -d "[$IDS]")
check_contains_str "Group delete returns isSuccess true" "$RESPONSE" '"isSuccess":true'

RESPONSE=$(curl -s -c $JAR -b $JAR -X POST "$BASE/delete-group-reservation" \
  -H "Content-Type: application/json" -H "RequestVerificationToken: $(token_from /Reservation/FilterReservation)" -d "[]")
check_contains_str "Group delete with empty selection is refused" "$RESPONSE" '"isSuccess":false'

RESPONSE=$(curl -s -c $JAR -b $JAR -X POST "$BASE/delete-group-reservation" \
  -H "Content-Type: application/json" -H "RequestVerificationToken: $(token_from /Reservation/FilterReservation)" -d "[999999]")
check_contains_str "Group delete reports unknown ids instead of failing" "$RESPONSE" 'was not found'

CODE=$(curl -s -b $JAR -X POST "$BASE/delete-group-reservation" \
  -H "Content-Type: application/json" -d "[1]" -o /dev/null -w "%{http_code}")
check "Group delete without antiforgery header is rejected" "$CODE" "400"

echo "############ RECORD: READ / FILTER ############"
curl -s -b $JAR "$BASE/Record/Index" -o /tmp/t_rec_list.html
check_contains "Record list shows badges" /tmp/t_rec_list.html "badge"
curl -s -b $JAR "$BASE/Record/Index?Status=Attended" -o /tmp/t_rec_attended.html
check_contains "Filter Status=Attended shows badge" /tmp/t_rec_attended.html "bg-success-lt"
curl -s -b $JAR "$BASE/Record/Index?Status=Cancelled" -o /tmp/t_rec_cancelled.html
check_contains "Filter Status=Cancelled shows badge" /tmp/t_rec_cancelled.html "bg-danger-lt"
curl -s -b $JAR "$BASE/Record/Index?Status=Reserved" -o /tmp/t_rec_reserved.html
check_contains "Filter Status=Reserved works" /tmp/t_rec_reserved.html "badge"
curl -s -b $JAR "$BASE/Record/Index?PaymentType=Cash" -o /tmp/t_rec_cash.html
check_contains "Filter PaymentType=Cash shows badge" /tmp/t_rec_cash.html "bg-green-lt"
curl -s -b $JAR "$BASE/Record/Index?PaymentType=CreditCard" -o /tmp/t_rec_cc.html
check_contains "Filter PaymentType=CreditCard shows badge" /tmp/t_rec_cc.html "bg-purple-lt"

# NotPaid used to fall through the switch and throw.
CODE=$(curl -s -b $JAR "$BASE/Record/Index?PaymentType=NotPaid" -o /tmp/t_rec_np.html -w "%{http_code}")
check "Filter PaymentType=NotPaid returns 200" "$CODE" "200"
check_contains "Filter PaymentType=NotPaid renders rows" /tmp/t_rec_np.html "edit-record/"

curl -s -b $JAR "$BASE/Record/Index?PatientName=Michael" -o /tmp/t_rec_name.html
check_contains "Filter by PatientName works" /tmp/t_rec_name.html "Michael Johnson"
curl -s -b $JAR "$BASE/Record/Index?PatientNationalId=1000000001" -o /tmp/t_rec_nid.html
check_contains "Filter by PatientNationalId works" /tmp/t_rec_nid.html "Michael Johnson"
curl -s -b $JAR "$BASE/Record/Index?Description=Routine" -o /tmp/t_rec_desc.html
check_contains "Filter by record Description works" /tmp/t_rec_desc.html "edit-record/"
curl -s -b $JAR "$BASE/Record/Index?ReservationId=1" -o /tmp/t_rec_resid.html
check_contains "Filter by ReservationId works" /tmp/t_rec_resid.html "edit-record/"

echo "############ RECORD: UPDATE ############"
curl -s -b $JAR "$BASE/Record/Index?PatientName=Michael&TakeEntity=5" -o /tmp/t_find_rec.html
REC_ID=$(grep -o 'edit-record/[0-9]*' /tmp/t_find_rec.html | head -1 | sed 's/edit-record\///')
echo "Record id for Michael=$REC_ID"
curl -s -c $JAR -b $JAR "$BASE/edit-record/$REC_ID" -o /tmp/t_er1.html
check_contains "Edit record form renders" /tmp/t_er1.html "PaidPrice"
TOKEN=$(get_token /tmp/t_er1.html)
CODE=$(curl -s -c $JAR -b $JAR -X POST "$BASE/edit-record/$REC_ID" \
  --data-urlencode "Id=$REC_ID" --data-urlencode "Status=Attended" --data-urlencode "PaymentType=CreditCard" \
  --data-urlencode "PaidPrice=200" --data-urlencode "Description=Edited via test" \
  --data-urlencode "__RequestVerificationToken=$TOKEN" \
  -o /tmp/t_er2.html -w "%{http_code}")
check "Edit record succeeds" "$CODE" "302"
curl -s -b $JAR "$BASE/Record/Index?PatientName=Michael" -o /tmp/t_er3.html
check_contains "Edited record shows CreditCard badge" /tmp/t_er3.html "bg-purple-lt"
curl -s -b $JAR "$BASE/Record/Index?PaidPrice=200&TakeEntity=100" -o /tmp/t_er4.html
check_contains "Edited record is findable by its new PaidPrice" /tmp/t_er4.html "edit-record/$REC_ID"

CODE=$(curl -s -b $JAR "$BASE/edit-record/999999" -o /dev/null -w "%{http_code}")
check "Edit form for unknown record returns 404" "$CODE" "404"

echo "############ RECORD: DELETE ############"
CODE=$(post_delete "/delete-record/$REC_ID" "/Record/Index" /tmp/t_delrec1.html)
check "Delete record succeeds" "$CODE" "302"
curl -s -b $JAR "$BASE/Record/Index?PaidPrice=200&TakeEntity=100" -o /tmp/t_delrec2.html
check_absent "Deleted record is gone from the list" /tmp/t_delrec2.html "edit-record/$REC_ID\""
CODE=$(post_delete "/delete-record/999999" "/Record/Index" /tmp/t_delrec3.html)
check "Delete unknown record degrades gracefully" "$CODE" "302"

echo "############ ADMIN: CREATE / READ / UPDATE / DELETE ############"
curl -s -b $JAR "$BASE/Admin/AdminsList" -o /tmp/t_al0.html
check_contains "Admin list shows the seeded admin" /tmp/t_al0.html "Default Admin"

TOKEN=$(token_from "/Admin/CreateAdmin")
CODE=$(curl -s -c $JAR -b $JAR -X POST "$BASE/Admin/CreateAdmin" \
  --data-urlencode "FullName=Test Admin Two" --data-urlencode "Mobile=09127770001" \
  --data-urlencode "__RequestVerificationToken=$TOKEN" \
  -o /tmp/t_ca2.html -w "%{http_code}")
check "Create admin succeeds" "$CODE" "302"

TOKEN=$(token_from "/Admin/CreateAdmin")
CODE=$(curl -s -c $JAR -b $JAR -X POST "$BASE/Admin/CreateAdmin" \
  --data-urlencode "FullName=Duplicate Admin" --data-urlencode "Mobile=09127770001" \
  --data-urlencode "__RequestVerificationToken=$TOKEN" \
  -o /tmp/t_ca3.html -w "%{http_code}")
check "Duplicate admin mobile is refused" "$CODE" "200"
check_contains "Duplicate admin shows error" /tmp/t_ca3.html "already exists"

curl -s -b $JAR "$BASE/Admin/AdminsList" -o /tmp/t_al1.html
ADMIN_ID=$(grep -B2 "Test Admin Two" /tmp/t_al1.html | grep -o 'UpdateAdmin/[0-9]*' | head -1 | sed 's/UpdateAdmin\///')
echo "New admin id=$ADMIN_ID"

curl -s -c $JAR -b $JAR "$BASE/Admin/UpdateAdmin/$ADMIN_ID" -o /tmp/t_ua1.html
check_contains "Update admin form is prefilled" /tmp/t_ua1.html "09127770001"
TOKEN=$(get_token /tmp/t_ua1.html)
CODE=$(curl -s -c $JAR -b $JAR -X POST "$BASE/Admin/UpdateAdmin" \
  --data-urlencode "Id=$ADMIN_ID" --data-urlencode "FullName=Test Admin Two Updated" \
  --data-urlencode "Mobile=09127770001" --data-urlencode "__RequestVerificationToken=$TOKEN" \
  -o /tmp/t_ua2.html -w "%{http_code}")
check "Update admin succeeds" "$CODE" "302"
curl -s -b $JAR "$BASE/Admin/AdminsList" -o /tmp/t_ua3.html
check_contains "Admin update reflected in list" /tmp/t_ua3.html "Test Admin Two Updated"

CODE=$(curl -s -b $JAR "$BASE/Admin/UpdateAdmin/999999" -o /dev/null -w "%{http_code}")
check "Update form for unknown admin returns 404" "$CODE" "404"

CODE=$(post_delete "/delete-admin/$ADMIN_ID" "/Admin/AdminsList" /tmp/t_da1.html)
check "Delete admin succeeds" "$CODE" "302"
curl -s -b $JAR "$BASE/Admin/AdminsList" -o /tmp/t_da2.html
check_absent "Admin removed after delete" /tmp/t_da2.html "UpdateAdmin/$ADMIN_ID\""
CODE=$(post_delete "/delete-admin/999999" "/Admin/AdminsList" /tmp/t_da3.html)
check "Delete unknown admin degrades gracefully" "$CODE" "302"

echo "############ AUTH: LOGOUT ############"
CODE=$(curl -s -c $JAR -b $JAR "$BASE/logout" -o /dev/null -w "%{http_code}")
check "Logout redirects" "$CODE" "302"
CODE=$(curl -s -b $JAR "$BASE/Patient/FilterPatients" -o /dev/null -w "%{http_code}")
check "Protected page redirects after logout" "$CODE" "302"

echo ""
echo "==================================="
echo "RESULTS: $PASS passed, $FAIL failed"
echo "==================================="
[ "$FAIL" -eq 0 ]
