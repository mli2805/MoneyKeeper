using AutoMapper;
using KeeperDomain;

namespace Keeper2018
{
    public class MappingEntitiesToModelsProfile : Profile
    {
        public MappingEntitiesToModelsProfile()
        {
            CreateMap<BankAccount, BankAccountModel>();

            CreateMap<Car, CarModel>();
            CreateMap<YearMileage, YearMileageModel>();

            CreateMap<DepoNewConds, DepoCondsModel>();

            CreateMap<CardBalanceMemo, CardBalanceMemoModel>();
        }
    }

    public class MappingModelsToEntitiesProfile : Profile
    {
        public MappingModelsToEntitiesProfile()
        {
            CreateMap<BankAccountModel, BankAccount>();

            CreateMap<CarModel, Car>();
            CreateMap<YearMileageModel, YearMileage>();

            CreateMap<DepoCondsModel, DepoNewConds>();

            CreateMap<CardBalanceMemoModel, CardBalanceMemo>();
        }
    }
}
