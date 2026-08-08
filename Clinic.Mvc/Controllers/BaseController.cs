using Clinic.Application.DTOs.Common;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Mvc.Controllers;

public class BaseController : Controller
{
   protected string ErrorMessage = "ErrorMessage";
   protected string SuccessMessage = "SuccessMessage";
   protected string InfoMessage = "InfoMessage";
   protected string WarningMessage = "WarningMessage";

   /// <summary>
   /// Combines a service response's summary message with any detailed
   /// per-item errors/warnings (e.g. batch operation conflicts) so nothing
   /// gets silently dropped when flashed to the user via TempData.
   /// </summary>
   protected static string BuildFlashMessage(BaseResponse response)
   {
      return response.Items is { Count: > 0 }
         ? $"{response.Message} {string.Join(" ", response.Items)}"
         : response.Message;
   }
}