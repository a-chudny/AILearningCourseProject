using MediatR;
using VolunteerPortal.API.Models.DTOs.Events;
using VolunteerPortal.API.Models.Enums;

namespace VolunteerPortal.API.Application.Events.Commands;

/// <summary>
/// Command to upload an image for an event.
/// </summary>
/// <param name="EventId">The ID of the event.</param>
/// <param name="File">The image file to upload.</param>
/// <param name="UserId">ID of the user performing the upload.</param>
/// <param name="UserRole">Role of the user performing the upload.</param>
public sealed record UploadEventImageCommand(
    int EventId,
    IFormFile File,
    int UserId,
    string UserRole) : IRequest<EventResponse>;
