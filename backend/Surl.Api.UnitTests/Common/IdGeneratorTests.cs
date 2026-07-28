using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Surl.Api.Common;
using Surl.Api.UnitTests.Helpers;
using Surl.Api.Users;

namespace Surl.Api.UnitTests.Common;

public class IdGeneratorTests
{
    private readonly IdOptions _idOptions;
    private readonly IdGenerator<User> _idGenerator;
    private readonly FakeTimeProvider _timeProvider;

    public IdGeneratorTests()
    {
        _timeProvider = new FakeTimeProvider(TestConstants.TestStartTime) // 2026-01-01
        {
            AutoAdvanceAmount = TimeSpan.FromTicks(1), // 1 millisecond = 1,000 microsecond = 10,000 ticks
        };
        _idOptions = new IdOptions
        {
            Epoch = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero), // 2025-01-01
            TimestampBits = 41,
            WorkerIdBits = 10,
            SequenceBits = 12,
            WorkerId = 1,
        };
        _idGenerator = new(Options.Create(_idOptions), _timeProvider);
    }

    [Fact]
    public void NewId_GeneratesSingleId_ShouldReturnPositiveId()
    {
        // Act
        long id = _idGenerator.NewId();

        // Assert
        Assert.True(id > 0);
    }

    [Fact]
    public void NewId_GeneratesSingleId_IdComponentsShouldBeValid()
    {
        // Act
        var id = new Id(_idGenerator.IdStructure, _idGenerator.NewId());

        // Assert
        var expectedTimestamp = TestConstants.TestStartTime.ToUnixTimeMilliseconds() - _idOptions.Epoch.ToUnixTimeMilliseconds();
        Assert.Equal(expectedTimestamp, id.Timestamp);
        Assert.Equal(_idOptions.WorkerId, id.WorkerId);
        Assert.Equal(0, id.Sequence);
    }

    [Fact]
    public void NewId_GeneratesMultipleIds_ReturnsUniqueIds()
    {
        // Arrange
        int count = 1000;
        var ids = new HashSet<long>();

        // Act
        for (int i = 0; i < count; i++)
        {
            long id = _idGenerator.NewId();
            ids.Add(id);
        }

        // Assert
        Assert.Equal(count, ids.Count); // all IDs should be unique
    }

    [Fact]
    public void NewId_GeneratesMultipleIds_IdsAreIncremental()
    {
        // Arrange
        int count = 999;
        long lastId = _idGenerator.NewId();

        // Act & Assert
        for (int i = 0; i < count; i++)
        {
            long id = _idGenerator.NewId();
            Assert.True(id > lastId); // IDs should be incremental
            lastId = id;
        }
    }

    [Fact]
    public void NewId_NextMillisecond_ResetsSequence()
    {
        // Arrange: generates a few IDs first.
        for (int i = 0; i < 1000; i++)
            _idGenerator.NewId();

        // Act
        _timeProvider.Advance(TimeSpan.FromMilliseconds(1)); // advance to next millisecond
        long id = _idGenerator.NewId();
        long sequence = new Id(_idGenerator.IdStructure, id).Sequence;

        // Assert
        Assert.Equal(0, sequence); // sequence reset to 0
    }

    [Fact]
    public void NewId_SequenceOverflow_WaitsForNextMillisecond()
    {
        // Arrange
        long lastIdValue = -1;
        for (int i = 0; i < 4096; i++)
            lastIdValue = _idGenerator.NewId();
        var lastId = new Id(_idGenerator.IdStructure, lastIdValue);

        // Act: next id will cause overflow
        var newId = new Id(_idGenerator.IdStructure, _idGenerator.NewId());

        // Assert
        Assert.Equal(0, newId.Sequence); // sequence reset to 0
        Assert.Equal(1, newId.Timestamp - lastId.Timestamp); // next millisecond
    }

    [Fact]
    public async Task NewId_MultipleThreads_GeneratesUniqueIds()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(TestConstants.TestStartTime)
        {
            AutoAdvanceAmount = TimeSpan.FromMicroseconds(1),
        };
        var idGenerator = new IdGenerator<User>(Options.Create(_idOptions), timeProvider);

        int threadCount = 10;
        int idsPerThread = 1000;
        var ids = new ConcurrentBag<long>();
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < threadCount; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < idsPerThread; j++)
                    ids.Add(idGenerator.NewId());
            }, TestContext.Current.CancellationToken));
        }
        await Task.WhenAll(tasks.ToArray());

        // Assert
        Assert.Equal(threadCount * idsPerThread, ids.Count); // all IDs should be generated
        Assert.Equal(threadCount * idsPerThread, ids.Distinct().Count()); // all IDs should be unique
    }
}
