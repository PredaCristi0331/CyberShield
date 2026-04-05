using CyberShield.Domain.Models;

namespace CyberShield.Domain.Contracts;

public interface IInferenceService
{
    string ModelVersion { get; }

    Task<IReadOnlyList<FrameResult>> RunBatchAsync(
        IReadOnlyList<TensorInput> batch,
        CancellationToken cancellationToken);
}