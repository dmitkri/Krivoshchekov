using System;
using System.Text.Json.Serialization;

namespace Lab1
{
    public class Person
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int Age { get; set; }
        
        [JsonIgnore]
        public string Password { get; set; } = string.Empty;
        
        [JsonPropertyName("personId")]
        public string Id { get; set; } = string.Empty;
        
        [JsonInclude]
        private DateTime _birthDate;
        
        [JsonIgnore]
        public DateTime BirthDate
        {
            get { return _birthDate; }
            set { _birthDate = value; }
        }
        
        private string _email = string.Empty;
        public string Email
        {
            get { return _email; }
            set
            {
                if (string.IsNullOrEmpty(value) || !value.Contains('@'))
                {
                    throw new ArgumentException("Email должен содержать символ '@'");
                }
                _email = value;
            }
        }
        
        [JsonPropertyName("phone")]
        public string PhoneNumber { get; set; } = string.Empty;
        
        public string FullName
        {
            get { return $"{FirstName} {LastName}"; }
        }
        
        public bool IsAdult
        {
            get { return Age >= 18; }
        }
    }
}

