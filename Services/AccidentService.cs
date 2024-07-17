using AutoMapper;
using FleetPulse_BackEndDevelopment.DTOs;
using FleetPulse_BackEndDevelopment.Models;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FleetPulse_BackEndDevelopment.Data;

namespace FleetPulse_BackEndDevelopment.Services
{
    public class AccidentService : IAccidentService
    {
        private readonly FleetPulseDbContext _context;
        private readonly IMapper _mapper;

        public AccidentService(FleetPulseDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AccidentDTO>> GetAllAccidentsAsync()
        {
            var accidents = await _context.Accidents.Include(a => a.Vehicle).ToListAsync();
            return _mapper.Map<IEnumerable<AccidentDTO>>(accidents);
        }

        public async Task<AccidentDTO> GetAccidentByIdAsync(int id)
        {
            var accident = await _context.Accidents.Include(a => a.Vehicle).FirstOrDefaultAsync(a => a.AccidentId == id);
            if (accident == null) return null;
            return _mapper.Map<AccidentDTO>(accident);
        }

        public async Task<AccidentDTO> CreateAccidentAsync(AccidentDTO accidentCreateDto)
        {
            var accident = _mapper.Map<Accident>(accidentCreateDto);

            // Handle photo compression and conversion
            if (accidentCreateDto.Photos != null && accidentCreateDto.Photos.Count > 0)
            {
                List<byte[]> photoBytesList = new List<byte[]>();

                foreach (var formFile in accidentCreateDto.Photos)
                {
                    if (formFile.Length > 0)
                    {
                        using var stream = formFile.OpenReadStream();
                        using var image = Image.Load(stream);
                        
                        // Compress image
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Mode = ResizeMode.Max,
                            Size = new Size(800, 800) // Resize to a reasonable size
                        }));

                        using var memoryStream = new MemoryStream();
                        image.Save(memoryStream, new JpegEncoder { Quality = 75 }); // Compress to 75% quality
                        photoBytesList.Add(memoryStream.ToArray());
                    }
                }

                accident.Photos = CombinePhotoBytes(photoBytesList);
            }

            _context.Accidents.Add(accident);
            await _context.SaveChangesAsync();

            return _mapper.Map<AccidentDTO>(accident);
        }

        public async Task<AccidentDTO> UpdateAccidentAsync(int id, AccidentDTO accidentDto)
        {
            var accident = await _context.Accidents.FindAsync(id);
            if (accident == null) return null;

            _mapper.Map(accidentDto, accident);
            _context.Accidents.Update(accident);
            await _context.SaveChangesAsync();

            return _mapper.Map<AccidentDTO>(accident);
        }

        public async Task<bool> DeactivateAccidentAsync(int id)
        {
            var accident = await _context.Accidents.FindAsync(id);
            if (accident == null) return false;

            accident.Status = false;
            _context.Accidents.Update(accident);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ActivateAccidentAsync(int id)
        {
            var accident = await _context.Accidents.FindAsync(id);
            if (accident == null) return false;

            accident.Status = true;
            _context.Accidents.Update(accident);
            await _context.SaveChangesAsync();

            return true;
        }

        private byte[] CombinePhotoBytes(List<byte[]> photoBytesList)
        {
            using var memoryStream = new MemoryStream();
            foreach (var photoBytes in photoBytesList)
            {
                memoryStream.Write(photoBytes, 0, photoBytes.Length);
            }
            return memoryStream.ToArray();
        }
    }
}
