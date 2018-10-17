using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace Hedgehog.Model.Extensions
{
    public static class CustomFormExtensions
    {
        public static string GetRichTextFormat(this IList<CustomForm> customForms)
        {
            if (customForms == null)
                return string.Empty;

            StringBuilder customFormsText = new StringBuilder();

            customFormsText.Append(
                @"{\rtf1\ansi\ansicpg1252\deff0{\fonttbl{\f0\fnil\fcharset0 Arial;}}{\colortbl;\red0\green0\blue0;\red255\green0\blue0;} \viewkind4\uc1\cf1\lang1033\fs20");

            foreach (CustomForm customForm in customForms.Where(cf => cf.Model != null))
            {
                foreach (CustomFormModelCategory category in customForm.Model.Categories.OrderBy(c => c))
                {
                    customFormsText.Append(@"\b\i ");
                    customFormsText.Append(category.Description);
                    customFormsText.Append(@"\i0\b0\line ");

                    foreach (CustomFormModelQuestion question in
                        category.Questions.Where(q => !q.ParentQuestionId.HasValue).OrderBy(q => q))
                    {
                        customFormsText.Append(GetQuestionDetails(category, question, customForm.Answers, @"\tab"));
                    }
                }
            }

            customFormsText.Append(@"}");

            return customFormsText.ToString();
        }

        private static string GetQuestionDetails(CustomFormModelCategory category, CustomFormModelQuestion question, IList<CustomFormAnswer> answers, string tabs)
        {
            if (category == null || question == null)
                return string.Empty;

            StringBuilder questionText = new StringBuilder();

            questionText.Append(tabs + @"\b ");
            questionText.Append(question.Description);
            questionText.Append(@"\b0\line ");

            if (answers != null && answers.Any(a => a.QuestionId == question.Id))
            {
                CustomFormAnswer answer = answers.Single(a => a.QuestionId == question.Id);

                CustomFormQuestionType type = answer.Question.QuestionType;

                questionText.Append(tabs + @"\tab ");
                questionText.Append(type == CustomFormQuestionType.Date || type == CustomFormQuestionType.DateTime
                    ? GetAnswerAsDateTime(answer)
                    : answer.Answer);
                questionText.Append(@"\line ");
            }

            List<CustomFormModelQuestion> children =
                category.Questions.Where(q => q.ParentQuestionId == question.Id).OrderBy(q => q).ToList();

            foreach (CustomFormModelQuestion customFormModelQuestion in children)
            {
                questionText.Append(GetQuestionDetails(category, customFormModelQuestion, answers, tabs + @"\tab"));
            }

            return questionText.ToString();
        }

        private static string GetAnswerAsDateTime(CustomFormAnswer answer)
        {
            if (string.IsNullOrWhiteSpace(answer?.Answer))
                return string.Empty;

            try
            {
                DateTime value = XmlConvert.ToDateTime(answer.Answer, XmlDateTimeSerializationMode.RoundtripKind);

                if (value.Ticks > 0 && answer.Question.QuestionType == CustomFormQuestionType.Date)
                {
                    return value.ToLocalTime().ToString("dd-MMM-yyyy");
                }

                if (value.Ticks > 0 && answer.Question.QuestionType == CustomFormQuestionType.DateTime)
                {
                    return value.ToLocalTime().ToString("dd-MMM-yyyy hh:mm tt");
                }

                return string.Empty;
            }
            catch (FormatException)
            {
                return string.Empty;
            }
        }
    }
}
