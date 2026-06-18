namespace PhysicianNotes.Domain.Notes;

public class SoapNote
{
    public string Subjective { get; set; } = string.Empty;

    public string Objective { get; set; } = string.Empty;

    public string Assessment { get; set; } = string.Empty;

    public string Plan { get; set; } = string.Empty;
}

//Subjective:
//Patient reports a persistent cough for three weeks with fatigue and intermittent fever.

//Objective:
//Low-grade fever reported.No other objective findings discussed.

//Assessment:
//Upper respiratory infection.

//Plan:
//Prescribe antibiotics, order chest X-ray, and follow up if symptoms worsen.