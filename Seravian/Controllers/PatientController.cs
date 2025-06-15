using System.Collections.Concurrent;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Digests;
using Seravian.DTOs.Doctor;

[Route("[controller]")]
[ApiController]
// [Authorize(Roles = "Patient")]
public class PatientController : ControllerBase
{
    [HttpGet("questions-answers")]
    public ActionResult<List<QuestionAnswerResponseDto>> QuestionsAnswers()
    {
        var QuestionsAnswersJsonPath = Path.Combine(
            "patient_helpful_ways",
            "realistic_mental_health_100_questions.json"
        );

        if (!System.IO.File.Exists(QuestionsAnswersJsonPath))
            return NotFound("File not found.");
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        return Ok(
            JsonSerializer.Deserialize<List<QuestionAnswerResponseDto>>(
                System.IO.File.ReadAllText(QuestionsAnswersJsonPath),
                options
            )
        );
    }

    [HttpGet("general-mental-health-disorders-Advices")]
    public ActionResult<
        List<GeneralMentalHealthDisordersAdvicesResponseDto>
    > GeneralMentalHealthDisordersAdvices()
    { //sugg
        var generalMentalHealthDisordersJsonPath = Path.Combine(
            "patient_helpful_ways",
            "mental_health_tips_by_disorder.json"
        );

        if (!System.IO.File.Exists(generalMentalHealthDisordersJsonPath))
            return NotFound("File not found.");
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        return Ok(
            JsonSerializer.Deserialize<List<GeneralMentalHealthDisordersAdvicesResponseDto>>(
                System.IO.File.ReadAllText(generalMentalHealthDisordersJsonPath),
                options
            )
        );
    }
}

public class QuestionAnswerResponseDto
{
    public int QuestionId { get; set; }
    public string Question { get; set; }
    public string Answer { get; set; }
}

public class GeneralMentalHealthDisordersAdvicesResponseDto
{
    public string Disorder { get; set; }
    public List<string> Advices { get; set; }
}
