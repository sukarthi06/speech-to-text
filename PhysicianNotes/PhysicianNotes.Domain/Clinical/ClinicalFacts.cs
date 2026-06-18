namespace PhysicianNotes.Domain.Clinical;

public class ClinicalFacts
{
    public string? ChiefComplaint { get; set; }

    public List<string> Symptoms { get; set; } = [];
    public List<string> RelevantHistory { get; set; } = [];

    public List<string> Medications { get; set; } = [];
    public List<string> Allergies { get; set; } = [];

    public List<string> Findings { get; set; } = [];
    public List<string> Diagnoses { get; set; } = [];

    public List<string> Orders { get; set; } = [];
    public List<string> FollowUpInstructions { get; set; } = [];
    public List<string> PatientEducation { get; set; } = [];
}

//ChiefComplaint:
//Persistent cough

//Symptoms:
//- Cough
//- Fatigue

//Diagnoses:
//- Upper respiratory infection

//Orders:
//- Chest X-ray

//FollowUpInstructions:
//- Return if symptoms worsen