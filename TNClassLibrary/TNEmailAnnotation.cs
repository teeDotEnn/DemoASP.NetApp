using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Web.Mvc;
using System.Net.Mail;

namespace TNClassLibrary
{
    /// <summary>
    /// the Validation Attribute for email addresses
    /// </summary>
    public class TNEmailAnnotation:ValidationAttribute
    {
        /// <summary>
        /// Set the error message of the attribute
        /// </summary>
        /// <param name="errorMessage">the error message to be displayed</param>
        public TNEmailAnnotation(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
        /// <summary>
        /// Check to see if the email is valid or not
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns>ValidationResult.Success if valid, Error message if not</returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ValidationResult result = ValidationResult.Success;
            if (value != null)
            {
                if (!String.IsNullOrEmpty(value.ToString()))
                {
                    try
                    {
                        MailAddress mail = new MailAddress(value.ToString());
                    }
                    catch (FormatException ex)
                    {
                        result = new ValidationResult(ErrorMessage);
                    }
                    catch (Exception ex)
                    {
                        result = new ValidationResult($"Unknown Error: {ex.Message}");
                    }
                }
                
            }
            
            return result;
        }
        


        
    }
}
