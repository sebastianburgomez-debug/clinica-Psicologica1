using Microsoft.EntityFrameworkCore;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Data.Seeders
{
    public class SeedDb
    {
        private readonly DataContext _context;
        private readonly IUsersService _usersService;
        private readonly IAccountService _accountService;

        public SeedDb(DataContext context, IUsersService usersService, IAccountService accountService)
        {
            _context = context;
            _usersService = usersService;
            _accountService = accountService;
        }

        public async Task SeedAsync()
        {
            // Crea la base de datos y aplica las migraciones pendientes
            await _context.Database.MigrateAsync();

            await new RolesSeeder(_context).SeedAsync();
            await new SpecialtiesSeeder(_context).SeedAsync();
            await new PermissionsSeeder(_context).SeedAsync();
            await new AccountSeeder(_usersService, _context, _accountService).SeedAsync();
        }
    }
}
