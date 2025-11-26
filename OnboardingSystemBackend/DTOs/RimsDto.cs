namespace OnboardingSystem.DTOs;


public class RimsPersonResponse
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<RimsEntity> Data { get; set; } = new();
}


public class RimsEntity
{
    public string Entity { get; set; } = string.Empty; // "Person" или "ADAccount"
    public RimsEntityResult Result { get; set; } = new();
}


public class RimsEntityResult
{
    public int PageSize { get; set; }
    public int PageNumber { get; set; }
    public int Total { get; set; }
    public List<RimsPersonItem> Items { get; set; } = new();
}


public class RimsPersonItem
{
    public string Uid { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty; // ФИО
    public string Company { get; set; } = string.Empty;
    public string CompanyPath { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Phones { get; set; } = string.Empty;
    public string CompanyType { get; set; } = string.Empty;
    public List<string> Emails { get; set; } = new();
    public List<RimsFlag> Flags { get; set; } = new();
}


public class RimsFlag
{
    public List<RimsSystemStatus>? SystemStatuses { get; set; }
    public bool? IsDrpDisabled { get; set; }
}


public class RimsSystemStatus
{
    public string Key { get; set; } = string.Empty;
    public RimsSystemStatusValue Value { get; set; } = new();
}


public class RimsSystemStatusValue
{
    public string AppName { get; set; } = string.Empty; // "DOMAIN.AD"
    public string System { get; set; } = string.Empty; // "AD"
    public bool Enabled { get; set; }
    public string Name { get; set; } = string.Empty; // "IvanovII"
    public string AccountId { get; set; } = string.Empty; // GUID
}


public class RimsAdAccountItem
{
    public string AccountId { get; set; } = string.Empty;
    public string Uid { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string AppName { get; set; } = string.Empty;
}



