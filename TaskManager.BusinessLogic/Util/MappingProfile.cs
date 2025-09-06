using AutoMapper;
using TaskManager.BusinessLogic.Dtos.Task;
using TaskManager.DataAccess.Models;

namespace TaskManager.BusinessLogic.Util;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<AppTask, TaskDto>();
        CreateMap<TaskCreateUpdateDto, AppTask>();
    }
}
