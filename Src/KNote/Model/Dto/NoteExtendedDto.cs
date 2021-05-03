using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto
{
    public class NoteExtendedDto : NoteDto
    {
        private List<ResourceDto> _resources;
        public List<ResourceDto> Resources
        {
            get
            {
                if (_resources == null)
                    _resources = new List<ResourceDto>();
                return _resources;
            }
            set
            {
                _resources = value;
            }
        }

        private List<Guid> _resourcesDeleted;
        public List<Guid> ResourcesDeleted
        {
            get
            {
                if (_resourcesDeleted == null)
                    _resourcesDeleted = new List<Guid>();
                return _resourcesDeleted;
            }
            set
            {
                _resourcesDeleted = value;
            }
        }

        private List<NoteTaskDto> _tasks;
        public List<NoteTaskDto> Tasks
        {
            get
            {
                if (_tasks == null)
                    _tasks = new List<NoteTaskDto>();
                return _tasks;
            }
            set
            {
                _tasks = value;
            }
        }

        private List<Guid> _tasksDeleted;
        public List<Guid> TasksDeleted
        {
            get
            {
                if (_tasksDeleted == null)
                    _tasksDeleted = new List<Guid>();
                return _tasksDeleted;
            }
            set
            {
                _tasksDeleted = value;
            }
        }

        private List<KMessageDto> _messages;
        public List<KMessageDto> Messages
        {
            get
            {
                if (_messages == null)
                    _messages = new List<KMessageDto>();
                return _messages;
            }
            set
            {
                _messages = value;
            }
        }

        private List<Guid> _messagesDeleted;
        public List<Guid> MessagesDeleted
        {
            get
            {
                if (_messagesDeleted == null)
                    _messagesDeleted = new List<Guid>();
                return _messagesDeleted;
            }
            set
            {
                _messagesDeleted = value;
            }
        }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationsResult = base.Validate(null);

            foreach (var a in KAttributesDto)
                validationsResult.Concat(a.Validate(null));

            foreach (var r in Resources)
                validationsResult.Concat(r.Validate(null));

            foreach (var t in Tasks)
                validationsResult.Concat(t.Validate(null));

            foreach (var m in Messages)
                validationsResult.Concat(m.Validate(null));

            return validationsResult;
        }

        public override void SetIsDirty(bool isDirty)
        {
            _isDirty = isDirty;

            foreach (var a in KAttributesDto)
                a.SetIsDirty(isDirty);

            foreach (var r in Resources)
                r.SetIsDirty(isDirty);

            foreach (var t in Tasks)
                t.SetIsDirty(isDirty);

            foreach (var m in Messages)
                m.SetIsDirty(isDirty);
        }

        public override void SetIsNew(bool isNew)
        {
            _isNew = isNew;

            foreach (var a in KAttributesDto)
                a.SetIsNew(isNew);

            foreach (var r in Resources)
                r.SetIsNew(isNew);

            foreach (var t in Tasks)
                t.SetIsNew(isNew);

            foreach (var m in Messages)
                m.SetIsNew(isNew);
        }

        public override bool IsDirty()
        {
            if (_isDirty)
                return true;

            foreach (var a in KAttributesDto)
                if (a.IsDirty())
                    return true;
            foreach (var r in Resources)
                if (r.IsDirty())
                    return true;
            foreach (var t in Tasks)
                if (t.IsDirty())
                    return true;
            foreach (var m in Messages)
                if (m.IsDirty())
                    return true;

            if(ResourcesDeleted.Count > 0)
                return true;
            if (TasksDeleted.Count > 0)
                return true;
            if (MessagesDeleted.Count > 0)
                return true;

            return false;
        }
    }
}
