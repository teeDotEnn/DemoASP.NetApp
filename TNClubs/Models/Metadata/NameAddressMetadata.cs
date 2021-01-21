using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TNClassLibrary;

namespace TNClubs.Models
{
    [ModelMetadataType(typeof(NameAddressMetadata))]
    public partial class NameAddress : IValidatableObject
    {
        private const int LENGTH_OF_VALID_POSTAL_CODE = 7;
        private const int INDEX_OF_CHAR_BEFORE_SPACE = 3;
        private const int PHONE_DIGITS_BLOCK_ONE = 3;
        private const int PHONE_DIGITS_BLOCK_TWO = 7;
        private const int PHONE_LENGTH = 10;
        private ClubsContext _context;   
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            _context = new ClubsContext();
            #region set null strings to empty
            if (FirstName == null)
            {
                FirstName = String.Empty;
            }
            if(LastName == null)
            {
                LastName = String.Empty;
            }
            if(CompanyName == null)
            {
                CompanyName = String.Empty;
            }
            if (StreetAddress == null)
            {
                StreetAddress = string.Empty;
            }
            if (City == null)
            {
                City = string.Empty;
            }
            if (PostalCode == null)
            {
                PostalCode = String.Empty;
            }
            if (ProvinceCode == null)
            {
                ProvinceCode = string.Empty;
            }
            if(Email == null)
            {
                Email = String.Empty;
            }
            if(Phone == null)
            {
                Phone = String.Empty;
            }
            #endregion
            #region Trim all strings
            FirstName = FirstName.Trim();
            LastName = LastName.Trim();
            CompanyName = CompanyName.Trim();
            StreetAddress = StreetAddress.Trim();
            City = City.Trim();
            PostalCode = PostalCode.Trim();
            Email = Email.Trim();
            Phone = Phone.Trim();
            #endregion
            #region Capitalize, firstName, lastName, CompanyName, st. Address, CIty
            FirstName = TNStringManipulation.TNCapitalize(FirstName);
            LastName = TNStringManipulation.TNCapitalize(LastName);
            CompanyName = TNStringManipulation.TNCapitalize(CompanyName);
            StreetAddress = TNStringManipulation.TNCapitalize(StreetAddress);
            City = TNStringManipulation.TNCapitalize(City);
            #endregion
            //Extract digits from phone number
            Phone = TNStringManipulation.TNExtractDigits(Phone);
            /*
            e.	At least one of FirstName, LastName or CompanyName must be specified.  All can be specified, but is not mandatory.
            f.	ProvinceCode is conditionally optional, but if provided: 
                i.	Validate it by fetching its record from the database … error if not found
                ii.	If fetching the province code throws an exception, put its innermost message into an error for that field.
                iii.	Retain the province record … it and its country are required to validate postalCode
           
            */
            if(String.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName) &&string.IsNullOrEmpty(CompanyName))
            {
                yield return new ValidationResult("Please enter a name");
            }


            if(!String.IsNullOrEmpty(ProvinceCode))
            {
                var provinceCode = _context.Province.Where(p => p.ProvinceCode == ProvinceCode).FirstOrDefault();
                if(provinceCode == null)
                {
                    yield return new ValidationResult("Invalid Province Code");
                }
                
            }
            
            /*
               g.	postalCode is conditionally optional but, if provided: 
                i.	Produce an error if provinceCode is invalid or empty … it’s required to edit a postal code
                ii.	Otherwise, fetch the country record for the given province.
                iii.	Use your XXStringManipulation.PostalCodeIsValid method to validate the postal code to the country’s postal pattern.
                iv.	Shift the postal code to upper case and, if the address is in Canada:
                    1.	Confirm the first letter is valid for the specified province
                    2.	Add a space in the middle, if it’s not there already
              
             */
            if(!String.IsNullOrEmpty(PostalCode))
            {
                //Check for a province code
                
                if(String.IsNullOrEmpty(ProvinceCode))
                {
                    yield return new ValidationResult("A province code is required if a postal code is provided");
                }
                else
                {
                    PostalCode = PostalCode.ToUpper();
                    NameAddress provinceCode = _context.NameAddress.Where(p => p.ProvinceCode.ToUpper() == ProvinceCode.ToUpper()).FirstOrDefault();
                    Province province = _context.Province.Where(p => p.ProvinceCode.ToUpper() == ProvinceCode.ToUpper()).FirstOrDefault();
                    Country country = _context.Country.Where(c => c.CountryCode.ToUpper() == province.CountryCode.ToUpper()).FirstOrDefault();
                    if(TNStringManipulation.TNPostalCodeIsValid(PostalCode,country.PostalPattern))
                    {
                        if(country.Name == "Canada")
                        {
                            if(!province.FirstPostalLetter.Contains(PostalCode[0]))
                            {
                                yield return new ValidationResult("Invalid first letter of postal code");
                            }
                            else
                            {
                                if(PostalCode.Length < LENGTH_OF_VALID_POSTAL_CODE)
                                {
                                    PostalCode = PostalCode.Insert(INDEX_OF_CHAR_BEFORE_SPACE, " ");
                                }
                            }
                        }
                    }
                    else
                    {
                        yield return new ValidationResult("Invalid Postal code format");
                    }
                }
            }
            /*
             h.	email is optional, but if provided, it must be a valid pattern:
                i.	Use your XXEmailAnnotation to validate it & provide the error message if not valid
                i.	If email is not provided, all the postal addressing information is required. 
                i.	Regardless of the email contents, all postal information that is provided must be validated & reformatted.
            j.	phone is required:
                i.	It must contain exactly 10 digits (use your class library method to remove punctuation and text).
                ii.	Reformat into dash notation: 519-748-5220
            */
            if(String.IsNullOrEmpty(Email))
            {
                if(String.IsNullOrEmpty(City)
                    || String.IsNullOrEmpty(StreetAddress)
                    || String.IsNullOrEmpty(PostalCode)
                    || String.IsNullOrEmpty(ProvinceCode))
                {
                    yield return new ValidationResult("If no email address is give, all postal information must be present");
                }
            }
            if (!String.IsNullOrEmpty(Phone))
            {
                if (Phone.Length == PHONE_LENGTH)
                {
                    Phone = Phone.Insert(PHONE_DIGITS_BLOCK_ONE, "-");
                    Phone = Phone.Insert(PHONE_DIGITS_BLOCK_TWO, "-");
                }
                else
                {
                    yield return new ValidationResult("Please enter 10 digts");
                }
            }
            yield return ValidationResult.Success;
        }
    }

    public class NameAddressMetadata
    {
        
        public int NameAddressId { get; set; }
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }
        [Display(Name = "Street Address")]
        
        public string StreetAddress { get; set; }
        public string City { get; set; }
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }
        [Display(Name = "Province Code")]
        public string ProvinceCode { get; set; }
        [TNEmailAnnotation("Invalid Email")]
        public string Email { get; set; }
        [Required]
        public string Phone { get; set; }
        public virtual Province ProvinceCodeNavigation { get; set; }
        public virtual Club Club { get; set; }
        public virtual ICollection<Artist> Artist { get; set; }
    }
}
