using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seravian.ActionFilters;

[Route("[controller]")]
[ApiController]
[Authorize(Roles = "Doctor")]
[UseVerifiedDoctorOnly]
public class DoctorSessionsController : ControllerBase { }
