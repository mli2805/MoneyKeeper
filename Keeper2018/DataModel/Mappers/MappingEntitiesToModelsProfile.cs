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
            CreateMap<CarYearMileage, YearMileageModel>();

            CreateMap<DepositConditions, DepoCondsModel>();

            CreateMap<CardBalanceMemo, CardBalanceMemoModel>();
        }
    }

    public class MappingModelsToEntitiesProfile : Profile
    {
        public MappingModelsToEntitiesProfile()
        {
            CreateMap<BankAccountModel, BankAccount>();

            CreateMap<CarModel, Car>();
            CreateMap<YearMileageModel, CarYearMileage>();

            CreateMap<DepoCondsModel, DepositConditions>();

            CreateMap<CardBalanceMemoModel, CardBalanceMemo>();
        }
    }
}
