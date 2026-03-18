public class PersonProfileInput
{
    public int PersonId { get; set; }

    public bool IncludeEmailInfo { get; set; } = true;
    public bool IncludeEntityInfo { get; set; } = true;
    public bool IncludeParentEntityInfo { get; set; } = false;
    public bool IncludeRelativeInfo { get; set; } = true;
    public bool IncludeAddressInfo { get; set; } = true;
    public bool IncludePhoneInfo { get; set; } = true;
}