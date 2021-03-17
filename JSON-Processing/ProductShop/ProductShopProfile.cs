﻿using AutoMapper;
using ProductShop.DAtaTransferObjects;
using ProductShop.Models;

namespace ProductShop
{
    public class ProductShopProfile : Profile
    {
        public ProductShopProfile()
        {
            
            this.CreateMap<UserInputModel, User>();
            this.CreateMap<ProductInputModel, Product>();
            this.CreateMap<CategoryInputModel, Category>();
            this.CreateMap<CategoryProductInputModel, CategoryProduct>();

        }
    }
}
