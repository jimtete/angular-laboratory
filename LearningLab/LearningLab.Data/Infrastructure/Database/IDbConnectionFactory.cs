using System.Data.Common;

namespace LearningLab.Data.Infrastructure.Database;

public interface IDbConnectionFactory
{
    DbConnection CreateConnection();
}
