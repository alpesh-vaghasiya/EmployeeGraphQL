using EmployeeGraphQL.Domain.Entities;

public class UserModuleAccessResponse
{
    public int UserId { get; set; }
    public int ModuleId { get; set; }
    public string ModuleName { get; set; } = null!;
    public string ModuleCode { get; set; } = null!;
    public int ActionId { get; set; }
    public string ActionName { get; set; } = null!;
    public string ActionCode { get; set; } = null!;
    public List<int> DepartmentId { get; set; }
}
public class PositionViewModel
{
    public int PositionId { get; set; }
    public string Name { get; set; }
    public string ShortName { get; set; }
    public int? RoleId { get; set; }
    public string? RoleName { get; set; }
    public int? GeoLevelId { get; set; }
    public int? EntityId { get; set; }
    public string? EntityName { get; set; }
    public int? Occurance { get; set; }
    public int? PersonId { get; set; }
    public string? PersonFName { get; set; }
    public string? PersonMName { get; set; }
    public string? PersonLName { get; set; }
    public string? PersonOName { get; set; }
    public List<PositionDepartment> Dept { get; set; }
}
public class RolePositionModel
{
    public int RoleId { get; set; }
    public int PositionId { get; set; }
}
public class DeptMandal
{
    public int DeptId { get; set; }
    public string? DeptName { get; set; }
    public int MandalId { get; set; }
    public string? MandalName { get; set; }
    public string? Wing { get; set; }
    public int DivisionId { get; set; }
}
public class DeptSCategory
{
    public int DeptId { get; set; }
    public string? DeptName { get; set; }

    public int SCatId { get; set; }
    public string? SCategoryName { get; set; }
    public string? SCatCode { get; set; }

    public string? Wing { get; set; }

    public int MandalId { get; set; }   // ⚠️ API gives string → we convert to int
    public string? MandalName { get; set; }

    public int DisplayOrder { get; set; }
}
public class PersonProfile
{
    public int personId { get; set; }
    public string fName { get; set; }
    public string mName { get; set; }
    public string lName { get; set; }
    public string bapsid { get; set; }
    public string gender { get; set; }
    public int mandalId { get; set; }
    public string hierarchyName { get; set; }
    public int familyId { get; set; }
    public bool isPrimary { get; set; }
    public int? sCatId { get; set; }
    public string? statusType { get; set; }
    public int? ageGroupYearE { get; set; }
    public int? ageGroupYearS { get; set; }
    public int? entityId { get; set; }
    public List<Phone> phone { get; set; }
    public List<Email> email { get; set; }
    public List<Address> address { get; set; }
    public RelativeInfo relativeInfo { get; set; }
    public EntityInfo entityInfo { get; set; }

    public string? _gender => gender?.ToLower() == "m" ? "Male" : gender?.ToLower() == "f" ? "Female" : "Other";
    public string? _phoneCell => phone?.FirstOrDefault(x => x.type?.ToLower() == "c")?.phone;
    public string? _isdCode => phone?.FirstOrDefault(x => x.type?.ToLower() == "c")?.isdCode;
    public string? _email => email?.FirstOrDefault(x => x.type?.ToLower() == "p")?.email;
    public string? _address => (address?.FirstOrDefault(x => x.type?.ToLower() == "h")?.addrLn1 + "") + (address?.FirstOrDefault(x => x.type?.ToLower() == "h")?.addrLn2 + "");
    public string? _city => address?.FirstOrDefault(x => x.type?.ToLower() == "h")?.cityTown;
    public string? _state => address?.FirstOrDefault(x => x.type?.ToLower() == "h")?.stateProvince;
    public string? _countryCode => address?.FirstOrDefault(x => x.type?.ToLower() == "h")?.countryCode;
    public string? _zipcode => address?.FirstOrDefault(x => x.type?.ToLower() == "h")?.postalCode;
    public int? _relationId => relativeInfo?.rTypeId;
    public string? _zone => entityInfo?.hierarchyName.Split("|")[hierarchyName.Split("|").Length - 1];
    public string? _center => entityInfo?.hierarchyName.Split("|")[hierarchyName.Split("|").Length - 2];
    public string? _region => entityInfo?.hierarchyName.Split("|")[hierarchyName.Split("|").Length - 3];
    public int _zoneId => entityInfo?.geoLevel == 60 ? Convert.ToInt32(entityInfo?.hierarchyId.Split("|")[entityInfo.hierarchyId.Split("|").Length - 1]) : 0;
    public int _centerId => entityInfo?.geoLevel == 60 ? Convert.ToInt32(entityInfo?.hierarchyId.Split("|")[entityInfo.hierarchyId.Split("|").Length - 2]) : entityInfo?.geoLevelId == 3 ? Convert.ToInt32(entityInfo?.hierarchyId.Split("|")[entityInfo.hierarchyId.Split("|").Length - 1]) : 0;
    public int _regionId => entityInfo?.geoLevel == 60 ? Convert.ToInt32(entityInfo?.hierarchyId.Split("|")[entityInfo.hierarchyId.Split("|").Length - 3]) : entityInfo?.geoLevelId == 3 ? Convert.ToInt32(entityInfo?.hierarchyId.Split("|")[entityInfo.hierarchyId.Split("|").Length - 2]) : 0;
    public int _divId => entityInfo != null ? entityInfo.DivId : 0;
}
public class Phone
{
    public string phone { get; set; }
    public string type { get; set; }
    public string? isdCode { get; set; }
}
public class Address
{
    public string type { get; set; }
    public string addrLn1 { get; set; }
    public object addrLn2 { get; set; }
    public string cityTown { get; set; }
    public string stateProvince { get; set; }
    public string postalCode { get; set; }
    public string countryCode { get; set; }
}
public class Email
{
    public string email { get; set; }
    public string type { get; set; }
}
public class RelativeInfo
{
    public int rPersonId { get; set; }
    public int rTypeId { get; set; }
}
public class EntityInfo
{
    public int geoLevelId { get; set; }
    public int geoLevel { get; set; }
    public string hierarchyId { get; set; }
    public string hierarchyName { get; set; }
    public int DivId { get; set; }
}
public class PositionDepartment
{
    public int DeptId { get; set; }
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Wing { get; set; }
    public int? PersonId { get; set; }
    public int? PositionId { get; set; }

}
public class RoleResponse
{
    public int RoleId { get; set; }
    public Guid RoleUUID { get; set; }
    public string Name { get; set; }
    public string ShortName { get; set; }
    public int DivId { get; set; }
    public int GeoLevelId { get; set; }

    public List<DepartmentDto> Dept { get; set; }
}
public class DepartmentDto
{
    public int DeptId { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string Wing { get; set; }
}