using System.Collections.Concurrent;
using System.Net.Mail;
using System.Security.Claims;
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
    [HttpGet("questions-answers")]
    public ActionResult<List<QuestionAnswerResponseDto>> QuestionsAnswers()
    {
        List<QuestionAnswerResponseDto> response =
        [
            new QuestionAnswerResponseDto
            {
                QuestionId = 1,
                Question = "lorem ims afdamkv mkldmam l;mdsa",
                Answer = "hoeap das;mas lorem",
            },
            new QuestionAnswerResponseDto
            {
                QuestionId = 2,
                Question = "lorem ims afdamkv mkldmam l;mdsa",
                Answer = "hoeap das;mas lorem",
            },
            new QuestionAnswerResponseDto
            {
                QuestionId = 3,
                Question = "lorem ims afdamkv mkldmam l;mdsa",
                Answer = "hoeap das;mas lorem",
            },
            new QuestionAnswerResponseDto
            {
                QuestionId = 4,
                Question = "lorem ims afdamkv mkldmam l;mdsa",
                Answer = "hoeap das;mas lorem",
            },
            new QuestionAnswerResponseDto
            {
                QuestionId = 5,
                Question = "lorem ims afdamkv mkldmam l;mdsa",
                Answer = "hoeap das;mas lorem",
            },
            new QuestionAnswerResponseDto
            {
                QuestionId = 6,
                Question = "lorem ims afdamkv mkldmam l;mdsa",
                Answer = "hoeap das;mas lorem",
            },
            new QuestionAnswerResponseDto
            {
                QuestionId = 7,
                Question = "lorem ims afdamkv mkldmam l;mdsa",
                Answer = "hoeap das;mas lorem",
            },
            new QuestionAnswerResponseDto
            {
                QuestionId = 8,
                Question = "lorem ims afdamkv mkldmam l;mdsa",
                Answer = "hoeap das;mas lorem",
            },
            new QuestionAnswerResponseDto
            {
                QuestionId = 9,
                Question = "lorem ims afdamkv mkldmam l;mdsa",
                Answer = "hoeap das;mas lorem",
            },
            new QuestionAnswerResponseDto
            {
                QuestionId = 10,
                Question = "lorem ims afdamkv mkldmam l;mdsa",
                Answer = "hoeap das;mas lorem",
            },
            new QuestionAnswerResponseDto
            {
                QuestionId = 11,
                Question = "lorem ims afdamkv mkldmam l;mdsa",
                Answer = "hoeap das;mas lorem",
            },
            new QuestionAnswerResponseDto
            {
                QuestionId = 12,
                Question = "lorem ims afdamkv mkldmam l;mdsa",
                Answer = "hoeap das;mas lorem",
            },
            new QuestionAnswerResponseDto
            {
                QuestionId = 13,
                Question = "lorem ims afdamkv mkldmam l;mdsa",
                Answer = "hoeap das;mas lorem",
            },
        ];
        return Ok(response);
    }
}

public class QuestionAnswerResponseDto
{
    public int QuestionId { get; set; }
    public string Question { get; set; }
    public string Answer { get; set; }
}
