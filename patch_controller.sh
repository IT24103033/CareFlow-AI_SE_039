awk '
/\[HttpGet\("\{id:guid\}"\)\]/ {
    print "        [Authorize(Roles = \"Patient,Doctor,Staff,Admin\")]"
    print "        [HttpGet(\"me\")]"
    print "        public async Task<IActionResult> GetMyProfile()"
    print "        {"
    print "            var patientClaim = User?.FindFirst(\"patient_id\")?.Value;"
    print "            if (!Guid.TryParse(patientClaim, out var authPatientId))"
    print "            {"
    print "                var fullName = User?.FindFirst(\"full_name\")?.Value;"
    print "                if (string.IsNullOrEmpty(fullName)) return Unauthorized();"
    print "                var profile = await _context.PatientProfiles"
    print "                    .Include(p => p.Admissions).ThenInclude(a => a.Ward)"
    print "                    .FirstOrDefaultAsync(p => p.FullName == fullName);"
    print "                if (profile == null) return NotFound(\"Profile not found.\");"
    print "                return Ok(profile);"
    print "            }"
    print "            var patient = await _context.PatientProfiles"
    print "                .Include(p => p.Admissions).ThenInclude(a => a.Ward)"
    print "                .FirstOrDefaultAsync(p => p.Id == authPatientId);"
    print "            if (patient == null) return NotFound();"
    print "            return Ok(patient);"
    print "        }"
    print ""
}
{ print }
' backend-api/Controllers/PatientProfilesController.cs > temp.cs && mv temp.cs backend-api/Controllers/PatientProfilesController.cs
