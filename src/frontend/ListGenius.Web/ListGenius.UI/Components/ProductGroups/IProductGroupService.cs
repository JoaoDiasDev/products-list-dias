﻿namespace ListGenius.UI.Components.ProductGroups;
public interface IProductGroupService
{
    Task<List<ProductGroupDto>> GetAll();
    Task<ProductGroupDto> GetById(int id);
}
