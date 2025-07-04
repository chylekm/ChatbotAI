using ChatbotAI.Application.Interfaces;

namespace ChatbotAI.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    public ITaskRepository Tasks { get; }

    public UnitOfWork(AppDbContext context, ITaskRepository taskRepository)
    {
        _context = context;
        Tasks = taskRepository;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}