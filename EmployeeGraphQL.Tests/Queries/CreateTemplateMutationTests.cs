using System.Net;
using FluentAssertions;
using Xunit;

public class CreateTemplateFrequencyTests : IClassFixture<TestFactory>
{
    private readonly GraphQLHelper _graphQL;

    public CreateTemplateFrequencyTests(TestFactory factory)
    {
        var client = factory.CreateClient();
        _graphQL = new GraphQLHelper(client);
    }

    private string BuildMutation(string frequencyConfig) => $@"
    mutation {{
        createTemplate(input: {{
            title: ""Test Template""
            projectTypeId: 1
            samparkTypeId: 1
            startDate: ""2026-04-01T00:00:00Z""
            endDate: ""2026-12-31T00:00:00Z""

            projectRepeateFrequencyConfig: {frequencyConfig}

            departmentConfigs:[
                {{departmentId:100, ownerRoleId:1, isPrimary:true}}
            ]
        }})
        {{
            title
        }}
    }}";

    //ONCE
    [Fact]
    public async Task Should_Create_Template_When_Once_Valid()
    {
        var config = @"
    {
        type:""once"",
        startDate:""2026-04-01T00:00:00Z"",
        endDate:""2026-04-30T00:00:00Z"",
        minDurationDays:5,
        maxDurationDays:20
    }";

        var response = await _graphQL.ExecuteMutation(BuildMutation(config));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    //negative maxDurationDays > project duration
    [Fact]
    public async Task Should_Return_Error_When_Once_MaxDuration_Invalid()
    {
        var config = @"
    {
        type:""once"",
        startDate:""2026-04-01T00:00:00Z"",
        endDate:""2026-04-05T00:00:00Z"",
        minDurationDays:5,
        maxDurationDays:20
    }";

        var response = await _graphQL.ExecuteMutation(BuildMutation(config));

        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("MaxDurationDays");
    }

    //REPEAT DAYS
    [Fact]
    public async Task Should_Create_Template_When_Repeat_Days_Valid()
    {
        var config = @"
    {
        type:""repeat"",
        startDate:""2026-04-01T00:00:00Z"",
        endDate:""2026-12-31T00:00:00Z"",
        repeatEvery:15,
        repeatUnit:""days"",
        minDurationDays:5,
        maxDurationDays:20
    }";

        var response = await _graphQL.ExecuteMutation(BuildMutation(config));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    // negative repeatEvery days invalid
    [Fact]
    public async Task Should_Return_Error_When_RepeatDays_Invalid()
    {
        var config = @"
    {
        type:""repeat"",
        startDate:""2026-04-01T00:00:00Z"",
        endDate:""2026-04-05T00:00:00Z"",
        repeatEvery:50,
        repeatUnit:""days"",
        minDurationDays:5,
        maxDurationDays:20
    }";

        var response = await _graphQL.ExecuteMutation(BuildMutation(config));

        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("RepeatEvery");
    }
    //REPEAT WEEKS
    [Fact]
    public async Task Should_Create_Template_When_Repeat_Weeks_Valid()
    {
        var config = @"
    {
        type:""repeat"",
        startDate:""2026-04-01T00:00:00Z"",
        endDate:""2026-12-31T00:00:00Z"",
        repeatEvery:2,
        repeatUnit:""weeks"",
        minDurationDays:7,
        maxDurationDays:30
    }";

        var response = await _graphQL.ExecuteMutation(BuildMutation(config));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    //REPEAT MONTHS
    [Fact]
    public async Task Should_Create_Template_When_Repeat_Monthly_Valid()
    {
        var config = @"
    {
        type:""repeat"",
        startDate:""2026-04-01T00:00:00Z"",
        endDate:""2026-12-31T00:00:00Z"",
        repeatEvery:1,
        repeatUnit:""months"",
        dayOfMonth:21,
        minDurationDays:7,
        maxDurationDays:30
    }";

        var response = await _graphQL.ExecuteMutation(BuildMutation(config));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    //invalid dayOfMonth
    [Fact]
    public async Task Should_Return_Error_When_DayOfMonth_Invalid()
    {
        var config = @"
    {
        type:""repeat"",
        startDate:""2026-04-01T00:00:00Z"",
        endDate:""2026-12-31T00:00:00Z"",
        repeatEvery:1,
        repeatUnit:""months"",
        dayOfMonth:40,
        minDurationDays:7,
        maxDurationDays:30
    }";

        var response = await _graphQL.ExecuteMutation(BuildMutation(config));

        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("StartOn");
    }
    //ADHOC
    [Fact]
    public async Task Should_Create_Template_When_Adhoc_Valid()
    {
        var config = @"
    {
        type:""adhoc"",
        startDate:""2026-04-01T00:00:00Z"",
        endDate:""2026-12-31T00:00:00Z"",
        createProjectTimes:3,
        adhocDates:[
            ""2026-04-10T00:00:00Z"",
            ""2026-05-15T00:00:00Z""
        ],
        minDurationDays:5,
        maxDurationDays:30
    }";

        var response = await _graphQL.ExecuteMutation(BuildMutation(config));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    //negative adhoc date outside duration
    [Fact]
    public async Task Should_Return_Error_When_Adhoc_Date_OutOfRange()
    {
        var config = @"
    {
        type:""adhoc"",
        startDate:""2026-04-01T00:00:00Z"",
        endDate:""2026-12-31T00:00:00Z"",
        createProjectTimes:3,
        adhocDates:[
            ""2027-01-01T00:00:00Z""
        ],
        minDurationDays:5,
        maxDurationDays:30
    }";

        var response = await _graphQL.ExecuteMutation(BuildMutation(config));

        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("Adhoc");
    }

    //invalid type
    [Fact]
    public async Task Should_Return_Error_When_Invalid_Type()
    {
        var config = @"
    {
        type:""daily"",
        startDate:""2026-04-01T00:00:00Z"",
        endDate:""2026-12-31T00:00:00Z"",
        repeatEvery:1,
        repeatUnit:""days"",
        minDurationDays:5,
        maxDurationDays:20
    }";

        var response = await _graphQL.ExecuteMutation(BuildMutation(config));

        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("Type must");
    }

    // endDate before startDate in frequency config
    [Fact]
    public async Task Should_Return_Error_When_EndDate_Before_StartDate()
    {
        var config = @"
    {
        type:""once"",
        startDate:""2026-04-30T00:00:00Z"",
        endDate:""2026-04-01T00:00:00Z"",
        minDurationDays:1,
        maxDurationDays:1
    }";

        var response = await _graphQL.ExecuteMutation(BuildMutation(config));
        var content  = await response.Content.ReadAsStringAsync();

        content.Should().Contain("End Date");
    }

    // once: minDurationDays > maxDurationDays
    [Fact]
    public async Task Should_Return_Error_When_Once_MinDuration_Exceeds_MaxDuration()
    {
        var config = @"
    {
        type:""once"",
        startDate:""2026-04-01T00:00:00Z"",
        endDate:""2026-04-30T00:00:00Z"",
        minDurationDays:25,
        maxDurationDays:10
    }";

        var response = await _graphQL.ExecuteMutation(BuildMutation(config));
        var content  = await response.Content.ReadAsStringAsync();

        content.Should().Contain("MinDurationDays");
    }

    // repeat: repeatUnit is not days | weeks | months
    [Fact]
    public async Task Should_Return_Error_When_Repeat_Invalid_Unit()
    {
        var config = @"
    {
        type:""repeat"",
        startDate:""2026-04-01T00:00:00Z"",
        endDate:""2026-12-31T00:00:00Z"",
        repeatEvery:5,
        repeatUnit:""hours"",
        minDurationDays:5,
        maxDurationDays:20
    }";

        var response = await _graphQL.ExecuteMutation(BuildMutation(config));
        var content  = await response.Content.ReadAsStringAsync();

        content.Should().Contain("Repeat Unit");
    }

    // repeat weeks: repeatEvery exceeds total weeks
    [Fact]
    public async Task Should_Return_Error_When_RepeatWeeks_Exceeds_Duration()
    {
        var config = @"
    {
        type:""repeat"",
        startDate:""2026-04-01T00:00:00Z"",
        endDate:""2026-04-10T00:00:00Z"",
        repeatEvery:5,
        repeatUnit:""weeks"",
        minDurationDays:1,
        maxDurationDays:5
    }";

        var response = await _graphQL.ExecuteMutation(BuildMutation(config));
        var content  = await response.Content.ReadAsStringAsync();

        content.Should().Contain("RepeatEvery");
    }

    // repeat months: repeatEvery exceeds total months
    [Fact]
    public async Task Should_Return_Error_When_RepeatMonths_Exceeds_Duration()
    {
        var config = @"
    {
        type:""repeat"",
        startDate:""2026-04-01T00:00:00Z"",
        endDate:""2026-06-30T00:00:00Z"",
        repeatEvery:5,
        repeatUnit:""months"",
        dayOfMonth:1,
        minDurationDays:7,
        maxDurationDays:30
    }";

        var response = await _graphQL.ExecuteMutation(BuildMutation(config));
        var content  = await response.Content.ReadAsStringAsync();

        content.Should().Contain("RepeatEvery");
    }

    // adhoc: createProjectTimes = 0
    [Fact]
    public async Task Should_Return_Error_When_Adhoc_CreateProjectTimes_Zero()
    {
        var config = @"
    {
        type:""adhoc"",
        startDate:""2026-04-01T00:00:00Z"",
        endDate:""2026-12-31T00:00:00Z"",
        createProjectTimes:0,
        adhocDates:[""2026-04-10T00:00:00Z""],
        minDurationDays:5,
        maxDurationDays:30
    }";

        var response = await _graphQL.ExecuteMutation(BuildMutation(config));
        var content  = await response.Content.ReadAsStringAsync();

        content.Should().Contain("Create Project Times");
    }

    // adhoc: maxDurationDays exceeds total duration
    [Fact]
    public async Task Should_Return_Error_When_Adhoc_MaxDuration_Exceeds_Duration()
    {
        var config = @"
    {
        type:""adhoc"",
        startDate:""2026-04-01T00:00:00Z"",
        endDate:""2026-04-05T00:00:00Z"",
        createProjectTimes:1,
        adhocDates:[""2026-04-03T00:00:00Z""],
        minDurationDays:1,
        maxDurationDays:20
    }";

        var response = await _graphQL.ExecuteMutation(BuildMutation(config));
        var content  = await response.Content.ReadAsStringAsync();

        content.Should().Contain("MaxDurationDays");
    }
}