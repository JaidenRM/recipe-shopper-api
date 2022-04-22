using AutoMapper;
using RecipeShopper.Domain;
using RecipeShopper.Domain.Enums;

namespace RecipeShopper.Infrastructure.Supermarkets.Woolworths
{
    public class WoolworthsMappingProfile : Profile
    {
        public WoolworthsMappingProfile()
        {
            CreateMap<WoolworthsProduct, Product>()
                .ForCtorParam(
                    "id",
                    opt => opt.MapFrom(src => src.Stockcode))
                .ForCtorParam(
                    "name",
                    opt => opt.MapFrom(src => src.Name))
                .ForCtorParam(
                    "currentPrice",
                    opt => opt.MapFrom(src => src.Price ?? 0))
                .ForCtorParam(
                    "fullPrice",
                    opt => opt.MapFrom(src => src.WasPrice ?? 0))
                .ForCtorParam(
                    "supermarket",
                    opt => opt.MapFrom(src => new Supermarket(SupermarketType.Woolworths, SupermarketType.Woolworths.ToFriendlyString())));
        }
    }
}
