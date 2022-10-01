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
    public void CheckForCreate_�����յ���ͬ()
    {
        var session = For<ISession>();
        Location start = new Location { LocationType = LocationTypes.K };
        Location end = start;
        Unitload unitload = new Unitload();
        TaskHelper sut = CreateTaskHelper(For<ISession>());

        var ex = Assert.Throws<CreateTaskFailedException>(() => sut.CheckForCreate(start, end, unitload, "TASKTYPE"));
        Assert.Equal("��������ʧ�ܣ������յ���ͬ��", ex.Message);
    }

    [Fact]
    public void CheckForCreate_������������()
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
        Assert.Equal("��������ʧ�ܣ�������������", ex.Message);
    }

    [Fact]
    public void CheckForCreate_��ͨ����_��㲻����()
    {
        var session = For<ISession>();
        Location start = new Location { LocationType = LocationTypes.K, OutboundLimit = 999, Exists = false };
        Location end = new Location { LocationType = LocationTypes.K, InboundLimit = 999 };
        Unitload unitload = new Unitload();

        TaskHelper sut = CreateTaskHelper(For<ISession>());

        var ex = Assert.Throws<CreateTaskFailedException>(() => sut.CheckForCreate(start, end, unitload, "TASKTYPE"));
        Assert.Equal("��������ʧ�ܣ���㲻���ڡ�", ex.Message);
    }


    [Fact]
    public void CheckForCreate_��ͨ����_����ֹ��վ()
    {
        var session = For<ISession>();
        Location start = new Location { LocationType = LocationTypes.K, OutboundLimit = 999, IsOutboundDisabled = true };
        Location end = new Location { LocationType = LocationTypes.K, InboundLimit = 999 };
        Unitload unitload = new Unitload();

        TaskHelper sut = CreateTaskHelper(For<ISession>());

        var ex = Assert.Throws<CreateTaskFailedException>(() => sut.CheckForCreate(start, end, unitload, "TASKTYPE"));
        Assert.Equal("��������ʧ�ܣ�����ֹ��վ��", ex.Message);
    }

    [Fact]
    public void CheckForCreate_��ͨ����_��������ֹ��վ()
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
        Assert.Equal("��������ʧ�ܣ���������ֹ��վ��", ex.Message);
    }

    [Fact]
    public void CheckForCreate_��ͨ����_���ﵽ��վ������()
    {
        var session = For<ISession>();
        Location start = new Location { LocationType = LocationTypes.K, OutboundLimit = 999, OutboundCount = 999 };
        Location end = new Location { LocationType = LocationTypes.K, InboundLimit = 999 };
        Unitload unitload = new Unitload();

        TaskHelper sut = CreateTaskHelper(For<ISession>());

        var ex = Assert.Throws<CreateTaskFailedException>(() => sut.CheckForCreate(start, end, unitload, "TASKTYPE"));
        Assert.Equal("��������ʧ�ܣ����ﵽ��վ�����ơ�", ex.Message);
    }

    
    [Fact]
    public void CheckForCreate_��ͨ����_������Nλ��ʱ������������Kλ��()
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
        Assert.Equal("��������ʧ�ܣ�������Nλ��ʱ������������Kλ�á�", ex.Message);
    }

    [Fact]
    public void CheckForCreate_��ͨ����_�������������λ�ò�ͬ()
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
        Assert.Equal("��������ʧ�ܣ��������������λ�ò�ͬ��", ex.Message);
    }


    [Fact]
    public void CheckForCreate_��������λ������_�������������λ�ò�ͬ()
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
        Assert.Equal("��������ʧ�ܣ��������������λ�ò�ͬ��", ex.Message);
    }


    [Fact]
    public void CheckForCreate_��ͨ����_�յ㲻����()
    {
        var session = For<ISession>();
        Location start = new Location { LocationType = LocationTypes.K, OutboundLimit = 999};
        Location end = new Location { LocationType = LocationTypes.K, InboundLimit = 999, Exists = false };
        Unitload unitload = new Unitload();

        TaskHelper sut = CreateTaskHelper(For<ISession>());

        var ex = Assert.Throws<CreateTaskFailedException>(() => sut.CheckForCreate(start, end, unitload, "TASKTYPE"));
        Assert.Equal("��������ʧ�ܣ��յ㲻���ڡ�", ex.Message);
    }

    [Fact]
    public void CheckForCreate_��ͨ����_�յ��ֹ��վ()
    {
        var session = For<ISession>();
        Location start = new Location { LocationType = LocationTypes.K, OutboundLimit = 999 };
        Location end = new Location { LocationType = LocationTypes.K, InboundLimit = 999, IsInboundDisabled = true };
        Unitload unitload = new Unitload();

        TaskHelper sut = CreateTaskHelper(For<ISession>());

        var ex = Assert.Throws<CreateTaskFailedException>(() => sut.CheckForCreate(start, end, unitload, "TASKTYPE"));
        Assert.Equal("��������ʧ�ܣ��յ��ֹ��վ��", ex.Message);
    }

    [Fact]
    public void CheckForCreate_��ͨ����_�յ������ֹ��վ()
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
        Assert.Equal("��������ʧ�ܣ��յ������ֹ��վ��", ex.Message);
    }

    [Fact]
    public void CheckForCreate_��ͨ����_�յ�ﵽ��վ������()
    {
        var session = For<ISession>();
        Location start = new Location { LocationType = LocationTypes.K, OutboundLimit = 999, };
        Location end = new Location { LocationType = LocationTypes.K, InboundLimit = 999, InboundCount = 999 };
        Unitload unitload = new Unitload();

        TaskHelper sut = CreateTaskHelper(For<ISession>());

        var ex = Assert.Throws<CreateTaskFailedException>(() => sut.CheckForCreate(start, end, unitload, "TASKTYPE"));
        Assert.Equal("��������ʧ�ܣ��յ�ﵽ��վ�����ơ�", ex.Message);
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
    public async Task CompleteAsync_����ʵ���յ��������յ㲻ͬ()
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
