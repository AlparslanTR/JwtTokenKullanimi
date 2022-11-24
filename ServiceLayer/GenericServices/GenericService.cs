using AutoMapper.Internal.Mappers;
using CoreLayer.GenericServices;
using CoreLayer.Repositories;
using CoreLayer.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.Mapper;
using SharedLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.GenericServices
{
    public class GenericService<TEntity, TDto> : IGenericService<TEntity, TDto> where TEntity : class where TDto : class
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<TEntity> _genericRepository;

        public GenericService(IUnitOfWork unitOfWork, IGenericRepository<TEntity> genericRepository)
        {
            _unitOfWork = unitOfWork;
            _genericRepository = genericRepository;
        }

        public async Task<Response<TDto>> AddAsync(TDto entity)
        {
            var newEntity = ObjectMapper.Mapper.Map<TEntity>(entity);
            await _genericRepository.AddAsync(newEntity);
            await _unitOfWork.SaveChangesAsync();
            var newDto= ObjectMapper.Mapper.Map<TDto>(newEntity);
            return Response<TDto>.Success(newDto,200);
        }

        public async Task<Response<IEnumerable<TDto>>> GetAllAsync()
        {
            var get = ObjectMapper.Mapper.Map<List<TDto>>(await _genericRepository.GetAllAsync());
            return Response<IEnumerable<TDto>>.Success(get, 200);
        }

        public async Task<Response<TDto>> GetByIdAsync(int id)
        {
            var getId= await _genericRepository.GetByIdAsync(id);
            if (getId==null)
            {
                return Response<TDto>.Fail("Id Bulunamadı", 404,true);
            }
            return Response<TDto>.Success(ObjectMapper.Mapper.Map<TDto>(getId), 200);
        }

        public async Task<Response<NoDataDto>> Remove(int id)
        {
            var remove = await _genericRepository.GetByIdAsync(id);
            if (remove==null)
            {
                return Response<NoDataDto>.Fail("Id Bulunamadı",404,true);
            }
            _genericRepository.Remove(remove);
            await _unitOfWork.SaveChangesAsync();
            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> Update(TDto entity,int id)
        {
            var update = await _genericRepository.GetByIdAsync(id);
            if (update==null)
            {
                return Response<NoDataDto>.Fail("Id Bulunamadı", 400, true);
            }
            var updateEntity = ObjectMapper.Mapper.Map<TEntity>(entity);
            _genericRepository.Update(updateEntity);
            await _unitOfWork.SaveChangesAsync();
            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<IQueryable<TDto>>> Where(Expression<Func<TEntity, bool>> predicate)
        {
            var list=_genericRepository.Where(predicate);
            return Response<IQueryable<TDto>>.Success(ObjectMapper.Mapper.Map<IQueryable<TDto>>(await list.ToListAsync()), 200);
        }
    }
}
