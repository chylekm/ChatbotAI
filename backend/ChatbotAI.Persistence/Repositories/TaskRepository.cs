using Microsoft.EntityFrameworkCore;
using ChatbotAI.Application.Interfaces;
using ChatbotAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatbotAI.Persistence.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default)
        => await _context.Tasks.AddAsync(task, cancellationToken);

    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Tasks.FindAsync(new object[] { id }, cancellationToken);

    public async Task<IEnumerable<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Tasks.ToListAsync(cancellationToken);

    public void Remove(TaskItem task)
        => _context.Tasks.Remove(task);
}