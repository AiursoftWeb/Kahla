using Aiursoft.Kahla.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Kahla.MySql;

public class MySqlContext(DbContextOptions<MySqlContext> options) : KahlaRelationalDbContext(options);
