using FluentValidation;
using LeaveFlow.Application.DTOs;

namespace LeaveFlow.Application.Features.LeaveRequests;

public class CreateLeaveRequestValidator : AbstractValidator<CreateLeaveRequestDto>
{
    public CreateLeaveRequestValidator()
    {
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty().GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be >= start date.");
        RuleFor(x => x.LeaveType).IsInEnum();
    }
}
