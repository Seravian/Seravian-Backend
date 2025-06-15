using System.Collections.Concurrent;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seravian.DTOs.Doctor;

[Route("[controller]")]
[ApiController]
[Authorize(Roles = "Patient")]
public class PatientController : ControllerBase
{
    [HttpGet("faq-questions-answers")]
    public ActionResult<List<QuestionAnswerResponseDto>> FaqQuestionsAnswers()
    {
        var faqQuestionsAnswersJsonPath =
            "QuestionsAndAnswers/realistic_mental_health_100_questions.json";

        return Ok(
            JsonSerializer.Deserialize<List<QuestionAnswerResponseDto>>(
                System.IO.File.ReadAllText(faqQuestionsAnswersJsonPath)
            )
        );
    }

    [HttpGet("questions-answers")]
    public ActionResult<List<QuestionAnswerResponseDto>> QuestionsAnswers()
    {
        var QuestionsAnswersJsonPath =
            "QuestionsAndAnswers/realistic_mental_health_100_questions.json";

        return Ok(
            JsonSerializer.Deserialize<List<QuestionAnswerResponseDto>>(
                System.IO.File.ReadAllText(QuestionsAnswersJsonPath)
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
