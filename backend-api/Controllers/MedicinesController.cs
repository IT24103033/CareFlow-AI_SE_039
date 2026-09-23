using System;
using System.Linq;
using System.Threading.Tasks;
using CareFlowAI.API.Data;
using CareFlowAI.API.DTOs;
using CareFlowAI.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareFlowAI.API.Controllers
{
    /// <summary>
    /// Component D – Medicine Catalog API
    /// Provides full CRUD, search, filter, sort, and pagination for the pharmacy inventory.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MedicinesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MedicinesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── GET /api/medicines ───────────────────────────────────────────────
        /// <summary>
        /// List all medicines with search, filter, sort and pagination.
        /// Query params:
        ///   search     - text search on Name or Category
        ///   filter     - "all" (default) | "lowstock" | "inactive" | "expiring"
        ///   sortBy     - "name" (default) | "stock" | "price" | "expiry"
        ///   sortDir    - "asc" (default) | "desc"
        ///   page       - page number (default 1)
        ///   pageSize   - items per page (default 10, max 50)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search   = null,
            [FromQuery] string  filter   = "all",
            [FromQuery] string  sortBy   = "name",
            [FromQuery] string  sortDir  = "asc",
            [FromQuery] int     page     = 1,
            [FromQuery] int     pageSize = 10)
        {
            pageSize = Math.Clamp(pageSize, 1, 50);
            page     = Math.Max(1, page);

            var query = _context.Medicines.AsQueryable();

            // ── Search ────────────────────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(m =>
                    m.Name.ToLower().Contains(s) ||
                    m.Category.ToLower().Contains(s) ||
                    m.Manufacturer.ToLower().Contains(s));
            }

            // ── Filter ────────────────────────────────────────────────────────
            query = filter.ToLower() switch
            {
                "lowstock"  => query.Where(m => m.IsActive && m.StockQuantity <= m.ReorderLevel),
                "inactive"  => query.Where(m => !m.IsActive),
                "expiring"  => query.Where(m => m.IsActive && m.ExpiryDate <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30))),
                _           => query.Where(m => m.IsActive), // default: only active
            };

            // ── Sort ──────────────────────────────────────────────────────────
            bool desc = sortDir.ToLower() == "desc";
            query = sortBy.ToLower() switch
            {
                "stock"  => desc ? query.OrderByDescending(m => m.StockQuantity) : query.OrderBy(m => m.StockQuantity),
                "price"  => desc ? query.OrderByDescending(m => m.UnitPrice)     : query.OrderBy(m => m.UnitPrice),
                "expiry" => desc ? query.OrderByDescending(m => m.ExpiryDate)    : query.OrderBy(m => m.ExpiryDate),
                _        => desc ? query.OrderByDescending(m => m.Name)          : query.OrderBy(m => m.Name),
            };

            // ── Paginate ──────────────────────────────────────────────────────
            int total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.Category,
                    m.Description,
                    m.Manufacturer,
                    m.StockQuantity,
                    m.ReorderLevel,
                    m.UnitPrice,
                    m.ExpiryDate,
                    m.IsActive,
                    m.CreatedAt,
                    m.UpdatedAt,
                    IsLowStock = m.StockQuantity <= m.ReorderLevel,
                    IsExpiringSoon = m.ExpiryDate <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30))
                })
                .ToListAsync();

            return Ok(new
            {
                TotalCount  = total,
                Page        = page,
                PageSize    = pageSize,
                TotalPages  = (int)Math.Ceiling(total / (double)pageSize),
                Items       = items
            });
        }

        // ── GET /api/medicines/{id} ──────────────────────────────────────────
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var medicine = await _context.Medicines.FindAsync(id);
            if (medicine is null) return NotFound(new { message = "Medicine not found." });
            return Ok(medicine);
        }

        // ── POST /api/medicines ──────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMedicineDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var medicine = new Medicine
            {
                Name          = dto.Name,
                Category      = dto.Category,
                Description   = dto.Description,
                Manufacturer  = dto.Manufacturer,
                StockQuantity = dto.StockQuantity,
                ReorderLevel  = dto.ReorderLevel,
                UnitPrice     = dto.UnitPrice,
                ExpiryDate    = dto.ExpiryDate,
                IsActive      = true
            };

            _context.Medicines.Add(medicine);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = medicine.Id }, medicine);
        }

        // ── PUT /api/medicines/{id} ──────────────────────────────────────────
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMedicineDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var medicine = await _context.Medicines.FindAsync(id);
            if (medicine is null) return NotFound(new { message = "Medicine not found." });

            if (dto.Name         is not null) medicine.Name         = dto.Name;
            if (dto.Category     is not null) medicine.Category     = dto.Category;
            if (dto.Description  is not null) medicine.Description  = dto.Description;
            if (dto.Manufacturer is not null) medicine.Manufacturer = dto.Manufacturer;
            if (dto.ReorderLevel is not null) medicine.ReorderLevel = dto.ReorderLevel.Value;
            if (dto.UnitPrice    is not null) medicine.UnitPrice    = dto.UnitPrice.Value;
            if (dto.ExpiryDate   is not null) medicine.ExpiryDate   = dto.ExpiryDate.Value;
            if (dto.IsActive     is not null) medicine.IsActive     = dto.IsActive.Value;

            medicine.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(medicine);
        }

        // ── DELETE /api/medicines/{id} (soft delete) ─────────────────────────
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var medicine = await _context.Medicines.FindAsync(id);
            if (medicine is null) return NotFound(new { message = "Medicine not found." });

            medicine.IsActive  = false;
            medicine.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Medicine '{medicine.Name}' has been deactivated." });
        }

        // ── PATCH /api/medicines/{id}/restock ────────────────────────────────
        /// <summary>Add units to the current stock quantity.</summary>
        [HttpPatch("{id:guid}/restock")]
        public async Task<IActionResult> Restock(Guid id, [FromBody] RestockMedicineDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var medicine = await _context.Medicines.FindAsync(id);
            if (medicine is null) return NotFound(new { message = "Medicine not found." });

            medicine.StockQuantity += dto.QuantityToAdd;
            medicine.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Stock updated. '{medicine.Name}' now has {medicine.StockQuantity} units.",
                NewStockQuantity = medicine.StockQuantity
            });
        }
    }
}
