using System;

namespace KNote.Model.Dto;

// What identifies a repository, read from its own database (SystemValues DB_REPOSITORYID and
// DB_REPOSITORYNAME): the Guid given to the database once, and the name the users see.
public class RepositoryInfoDto
{
    public Guid RepositoryId { get; set; }
    public string Name { get; set; }
}
