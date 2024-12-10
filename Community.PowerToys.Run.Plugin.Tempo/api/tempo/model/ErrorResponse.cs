namespace PowerToys_Run_Tempo.api.tempo.model;

public class ErrorResponse(
    Error[] errors
)
{
    public Error[] errors { get; } = errors;
}