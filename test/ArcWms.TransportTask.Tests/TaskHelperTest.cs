using Arc.AppSeqs;
using Autofac.Features.Indexed;
using Microsoft.Extensions.Logging;
using NHibernate;
using Xunit.Abstractions;

namespace ArcWms.Tests;

public class TaskHelperTest
{
    private class Seqs : IAppSeqService
    {
        int next = 0;

        public Seqs()
        {
        }
        public Task<int> GetNextAsync(string seqName)
        {
            next++;
            return Task.FromResult(next);

        }
    }


    public TaskHelperTest(ITestOutputHelper output)
    {
    }

    private UnitloadHelper CreateUnitloadHelper(ISession session)
    {
        return new UnitloadHelper(session, () => new UnitloadSnapshot(), () => new UnitloadItemSnapshot(), For<ILogger<UnitloadHelper>>());
    }
    private TaskHelper CreateTaskHelper(ISession session)
    {
        var unitloadHelper = CreateUnitloadHelper(session);
        return new TaskHelper(session, new Seqs(), unitloadHelper, 
            () => new TransportTask(), 
            () => new ArchivedTransportTask(),
            For<IIndex<string, IRequestHandler>>(),
            For<IIndex<string, ICompletionHandler>>(),
            For<ILogger<TaskHelper>>()
            );
    }

    [Fact]
    public void CheckForCreate_起点和终点相同()
    {
        var session = For<ISession>();
        Location start = new Location { LocationType = LocationTypes.K };
        Location end = start;
        Unitload unitload = new Unitload();
        TaskHelper sut = CreateTaskHelper(For<ISession>());

        var ex = Assert.Throws<CreateTaskFailedException>(() => sut.CheckForCreate(start, end, unitload, "TASKTYPE"));
        Assert.Equal("创建任务失败：起点和终点相同。", ex.Message);
    }

    [Fact]
    public void CheckForCreate_托盘已有任务()
    {
        var session = For<ISession>();
        Location start = new Location { LocationType = LocationTypes.K, OutboundLimit = 999 };
        Location end = new Location { LocationType = LocationTypes.K, InboundLimit = 999 };
        Unitload unitload = new Unitload
        {
            HasTask = true,
        };

        TaskHelper sut = CreateTaskHelper(For<ISession>());

        var ex = Assert.Throws<CreateTaskFailedException>(() => sut.CheckForCreate(start, end, unitload, "TASKTYPE"));
        Assert.Equal("创建任务失败：托盘已有任务。", ex.Message);
    }

    [Fact]
    public void CheckForCreate_普通任务_起点不存在()
    {
        var session = For<ISession>();
        Location start = new Location { LocationType = LocationTypes.K, OutboundLimit = 999, Exists = false };
        Location end = new Location { LocationType = LocationTypes.K, InboundLimit = 999 };
        Unitload unitload = new Unitload();

        TaskHelper sut = CreateTaskHelper(For<ISession>());

        var ex = Assert.Throws<CreateTaskFailedException>(() => sut.CheckForCreate(start, end, unitload, "TASKTYPE"));
        Assert.Equal("创建任务失败：起点不存在。", ex.Message);
    }


    [Fact]
    public void CheckForCreate_普通任务_起点禁止出站()
    {
        var session = For<ISession>();
        Location start = new Location { LocationType = LocationTypes.K, OutboundLimit = 999, IsOutboundDisabled = true };
        Location end = new Location { LocationType = LocationTypes.K, InboundLimit = 999 };
        Unitload unitload = new Unitload();

        TaskHelper sut = CreateTaskHelper(For<ISession>());

        var ex = Assert.Throws<CreateTaskFailedException>(() => sut.CheckForCreate(start, end, unitload, "TASKTYPE"));
        Assert.Equal("创建任务失败：起点禁止出站。", ex.Message);
    }

    [Fact]
    public void CheckForCreate_普通任务_起点巷道禁止出站()
    {
        var session = For<ISession>();
        Location start = new Location 
        {
            LocationType = LocationTypes.K, 
            OutboundLimit = 999, 
            Streetlet = new Streetlet
            {
                IsOutboundDisabled = true 
            }
        };
        Location end = new Location { LocationType = LocationTypes.K, InboundLimit = 999 };
        Unitload unitload = new Unitload();

        TaskHelper sut = CreateTaskHelper(For<ISession>());

        var ex = Assert.Throws<CreateTaskFailedException>(() => sut.CheckForCreate(start, end, unitload, "TASKTYPE"));
        Assert.Equal("创建任务失败：起点巷道禁止出站。", ex.Message);
    }

    [Fact]
    public void CheckForCreate_普通任务_起点达到出站数限制()
    {
        var session = For<ISession>();
        Location start = new Location { LocationType = LocationTypes.K, OutboundLimit = 999, OutboundCount = 999 };
        Location end = new Location { LocationType = LocationTypes.K, InboundLimit = 999 };
        Unitload unitload = new Unitload();

        TaskHelper sut = CreateTaskHelper(For<ISession>());

        var ex = Assert.Throws<CreateTaskFailedException>(() => sut.CheckForCreate(start, end, unitload, "TASKTYPE"));
        Assert.Equal("创建任务失败：起点达到出站数限制。", ex.Message);
    }

    
    [Fact]
    public void CheckForCreate_普通任务_托盘在N位置时任务起点必须是K位置()
    {
        var session = For<ISession>();
        Location start = new Location { LocationType = LocationTypes.N, OutboundLimit = 999 };
        Location end = new Location { LocationType = LocationTypes.K, InboundLimit = 999 };
        Unitload unitload = new Unitload
        {
            CurrentLocation = new Location
            {
                LocationType = "N"
            }
        };

        TaskHelper sut = CreateTaskHelper(For<ISession>());
        var ex = Assert.Throws<CreateTaskFailedException>(() => sut.CheckForCreate(start, end, unitload, "TASKTYPE"));
        Assert.Equal("创建任务失败：托盘在N位置时任务起点必须是K位置。", ex.Message);
    }

    [Fact]
    public void CheckForCreate_普通任务_起点与托盘所在位置不同()
    {
        var session = For<ISession>();
        Location start = new Location { LocationType = LocationTypes.K, OutboundLimit = 999 };
        Location end = new Location { LocationType = LocationTypes.K, InboundLimit = 999 };
        Unitload unitload = new Unitload
        {
            CurrentLocation = new Location
            {
                LocationType = "K"
            }
        };

        TaskHelper sut = CreateTaskHelper(For<ISession>());
        var ex = Assert.Throws<CreateTaskFailedException>(() => sut.CheckForCreate(start, end, unitload, "TASKTYPE"));
        Assert.Equal("创建任务失败：起点与托盘所在位置不同。", ex.Message);
    }


    [Fact]
    public void CheckForCreate_更改托盘位置任务_起点与托盘所在位置不同()
    {
        var session = For<ISession>();
        Location start = new Location { LocationType = LocationTypes.K, OutboundLimit = 999 };
        Location end = new Location { LocationType = LocationTypes.K, InboundLimit = 999 };
        Unitload unitload = new Unitload
        {
            CurrentLocation = new Location
            {
                LocationType = "K"
            }
        };

        TaskHelper sut = CreateTaskHelper(For<ISession>());
        var ex = Assert.Throws<CreateTaskFailedException>(() => sut.CheckForCreate(start, end, unitload, TaskHelper.ChangeUnitloadLocationTaskType));
        Assert.Equal("创建任务失败：起点与托盘所在位置不同。", ex.Message);
    }


    [Fact]
    public void CheckForCreate_普通任务_终点不存在()
    {
        var session = For<ISession>();
        Location start = new Location { LocationType = LocationTypes.K, OutboundLimit = 999};
        Location end = new Location { LocationType = LocationTypes.K, InboundLimit = 999, Exists = false };
        Unitload unitload = new Unitload();

        TaskHelper sut = CreateTaskHelper(For<ISession>());

        var ex = Assert.Throws<CreateTaskFailedException>(() => sut.CheckForCreate(start, end, unitload, "TASKTYPE"));
        Assert.Equal("创建任务失败：终点不存在。", ex.Message);
    }

    [Fact]
    public void CheckForCreate_普通任务_终点禁止入站()
    {
        var session = For<ISession>();
        Location start = new Location { LocationType = LocationTypes.K, OutboundLimit = 999 };
        Location end = new Location { LocationType = LocationTypes.K, InboundLimit = 999, IsInboundDisabled = true };
        Unitload unitload = new Unitload();

        TaskHelper sut = CreateTaskHelper(For<ISession>());

        var ex = Assert.Throws<CreateTaskFailedException>(() => sut.CheckForCreate(start, end, unitload, "TASKTYPE"));
        Assert.Equal("创建任务失败：终点禁止入站。", ex.Message);
    }

    [Fact]
    public void CheckForCreate_普通任务_终点巷道禁止入站()
    {
        var session = For<ISession>();
        Location start = new Location { LocationType = LocationTypes.K, OutboundLimit = 999, };
        Location end = new Location
        {
            LocationType = LocationTypes.K,
            InboundLimit = 999,
            Streetlet = new Streetlet
            {
                IsInboundDisabled = true
            }
        };
        Unitload unitload = new Unitload();

        TaskHelper sut = CreateTaskHelper(For<ISession>());

        var ex = Assert.Throws<CreateTaskFailedException>(() => sut.CheckForCreate(start, end, unitload, "TASKTYPE"));
        Assert.Equal("创建任务失败：终点巷道禁止入站。", ex.Message);
    }

    [Fact]
    public void CheckForCreate_普通任务_终点达到入站数限制()
    {
        var session = For<ISession>();
        Location start = new Location { LocationType = LocationTypes.K, OutboundLimit = 999, };
        Location end = new Location { LocationType = LocationTypes.K, InboundLimit = 999, InboundCount = 999 };
        Unitload unitload = new Unitload();

        TaskHelper sut = CreateTaskHelper(For<ISession>());

        var ex = Assert.Throws<CreateTaskFailedException>(() => sut.CheckForCreate(start, end, unitload, "TASKTYPE"));
        Assert.Equal("创建任务失败：终点达到入站数限制。", ex.Message);
    }



    [Fact]
    public async Task CreateAsyncTest()
    {
        var session = For<ISession>();
        Location start = new Location { LocationType = LocationTypes.K, OutboundLimit = 999 };
        Location end = new Location { LocationType = LocationTypes.K, InboundLimit = 999 };
        Unitload unitload = new Unitload();
        TaskHelper sut = CreateTaskHelper(session);
        
        TransportTask transportTask = await sut.CreateAsync("TASKTYPE", start, end, unitload);

        Assert.Equal(1, start.OutboundCount);
        Assert.Equal(1, end.InboundCount);
        Assert.Equal("TASKTYPE", transportTask.TaskType);
        Assert.Same(unitload, transportTask.Unitload);
        Assert.Same(start, transportTask.Start);
        Assert.Same(end, transportTask.End);
        Assert.True(unitload.HasTask);
        await session.Received().SaveAsync(transportTask);
    }

    [Fact]
    public async Task CompleteAsyncTest()
    {
        var session = For<ISession>();
        Location start = new Location { LocationType = LocationTypes.S, OutboundLimit = 999 };
        Location end = new Location { LocationType = LocationTypes.K, InboundLimit = 999 };
        Unitload unitload = new Unitload();
        await CreateUnitloadHelper(session).EnterAsync(unitload, start);
        TaskHelper sut = CreateTaskHelper(session);

        TransportTask transportTask = await sut.CreateAsync("TASKTYPE", start, end, unitload);

        await sut.CompleteAsync(transportTask, end);

        Assert.Equal(0, start.OutboundCount);
        Assert.Equal(0, end.InboundCount);
        Assert.False(unitload.HasTask);
        Assert.Same(end, unitload.CurrentLocation);
        await session.Received().DeleteAsync(Arg.Any<TransportTask>());
        await session.Received().SaveAsync(Arg.Any<ArchivedTransportTask>());
    }


    [Fact]
    public async Task CompleteAsync_允许实际终点与任务终点不同()
    {
        var session = For<ISession>();
        Location start = new Location { LocationType = LocationTypes.S, OutboundLimit = 999 };
        Location end = new Location { LocationType = LocationTypes.K, InboundLimit = 999 };
        Unitload unitload = new Unitload();
        await CreateUnitloadHelper(session).EnterAsync(unitload, start);
        TaskHelper sut = CreateTaskHelper(session);
        var actualEnd = new Location { LocationType = LocationTypes.K, InboundLimit = 999 };

        TransportTask transportTask = await sut.CreateAsync("TASKTYPE", start, end, unitload);
        await sut.CompleteAsync(transportTask, actualEnd);

        Assert.Equal(0, start.OutboundCount);
        Assert.Equal(0, end.InboundCount);
        Assert.False(unitload.HasTask);
        await session.Received().DeleteAsync(Arg.Any<TransportTask>());
        await session.Received().SaveAsync(Arg.Any<ArchivedTransportTask>());
        Assert.Same(actualEnd, unitload.CurrentLocation);
    }


    [Fact]
    public async Task CancelAsyncTest()
    {
        var session = For<ISession>();
        Location start = new Location { LocationType = LocationTypes.S, OutboundLimit = 999 };
        Location end = new Location { LocationType = LocationTypes.K, InboundLimit = 999 };
        Unitload unitload = new Unitload();
        await CreateUnitloadHelper(session).EnterAsync(unitload, start);
        TaskHelper sut = CreateTaskHelper(session);

        TransportTask transportTask = await sut.CreateAsync("TASKTYPE", start, end, unitload);

        await sut.CancelAsync(transportTask);

        Assert.Equal(0, start.OutboundCount);
        Assert.Equal(0, end.InboundCount);
        Assert.False(unitload.HasTask);
        await session.Received().DeleteAsync(Arg.Any<TransportTask>());
        await session.Received().SaveAsync(Arg.Any<ArchivedTransportTask>());
        Assert.Same(start, unitload.CurrentLocation);
    }
}
