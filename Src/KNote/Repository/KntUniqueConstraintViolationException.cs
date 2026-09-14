using System;

namespace KNote.Repository;

// Thrown by Repository.Dapper/Repository.EntityFramework instead of the generic
// KntRepositoryException when an insert collides with a unique index/constraint. Lets callers
// outside those projects (e.g. Service) branch on "this specific failure is a concurrent-write
// conflict, retry" without referencing DB-provider exception types (Repository.Dapper and
// Repository.EntityFramework each have their own provider-aware detection - see
// DbExceptionExtensions.IsUniqueConstraintViolation in each project - which Service cannot see,
// both because they are internal and because Service should not need to know which ORM/provider is
// active) and without the fragility of matching on exception message text.
public class KntUniqueConstraintViolationException : KntRepositoryException
{
    public KntUniqueConstraintViolationException() : base() { }

    public KntUniqueConstraintViolationException(string message) : base(message) { }

    public KntUniqueConstraintViolationException(string message, Exception inner) : base(message, inner) { }
}
