using ApiWeb.DTOs.Products;
using ApiWeb.Services.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiWeb.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<ProductResponse>>> GetAll(
            [FromQuery] ProductQueryParams queryParams,
            CancellationToken ct)
        {
            var response = await _productService.GetAllAsync(queryParams, ct);
            return Ok(response);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductResponse>> GetById([FromRoute] int id, CancellationToken ct)
        {
            var response = await _productService.GetByIdAsync(id, ct);

            if (response is null)
                return NotFound("Product not found");

            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<ProductResponse>> Create(
            [FromBody] CreateProductRequest request,
            CancellationToken ct)
        {
            var response = await _productService.CreateAsync(request, ct);

            return CreatedAtAction(
                nameof(GetById),
                new { id = response.Id },
                response
            );
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ProductResponse>> Update(
            [FromRoute] int id,
            [FromBody] UpdateProductRequest request,
            CancellationToken ct)
        {
            var response = await _productService.UpdateAsync(id, request, ct);

            if (response is null)
                return NotFound();

            return Ok(response);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            var deleted = await _productService.DeleteAsync(id, ct);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}