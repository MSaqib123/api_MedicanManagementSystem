namespace api_MedicanManagementSystem.Models
{
    // DTO for translations (Frontend se aayega)
    public class EntityTranslationDto
    {
        public string PropertyName { get; set; }  // "Name", "Composition", "Dosage"
        public string LanguageCode { get; set; }  // "en-US", "ur-PK", "sd-PK"
        public string Value { get; set; }
    }
}
