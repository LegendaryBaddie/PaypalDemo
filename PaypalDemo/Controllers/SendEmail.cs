using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaypalDemo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SendEmail : ControllerBase
    {
        static async Task<string> sendMail()
        {
            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("austin.white@bss.group", "Austin");
            var subject = "Email to multiple recipients";
            //create a to email
            var plainTextContent = "This is some sample text that is the main body";
            var htmlContent = "<h1>Ciao,</h1><strong>I also forgot to mention that they provide an api to validate an email</strong>";
            //var msg = MailHelper.CreateSingleEmail(from, from, subject, plainTextContent, htmlContent);
            List<EmailAddress> emails = new List<EmailAddress>();
            emails.Add(new EmailAddress("austin.white@bss.group", "Austin"));
            emails.Add(new EmailAddress("SomeBad@Email.Address", ""));
            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, emails, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
            var s_resp = await response.Body.ReadAsStringAsync();
            return s_resp;
        }

        [HttpGet]
        public async Task<string> Send()
        {
            var attempt = await sendMail();

            return attempt;
        }
    }
}
