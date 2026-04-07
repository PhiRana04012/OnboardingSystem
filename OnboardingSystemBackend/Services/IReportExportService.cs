using OnboardingSystem.DTOs;

namespace OnboardingSystem.Services;

public interface IReportExportService
{
    byte[] ExportOnboardingProgressToExcel(OnboardingProgressReportDto report);
    byte[] ExportOnboardingProgressToPdf(OnboardingProgressReportDto report);
    
    byte[] ExportTestResultsToExcel(List<TestResultsReportDto> report);
    byte[] ExportTestResultsToPdf(List<TestResultsReportDto> report);
    
    byte[] ExportDepartmentReportToExcel(DepartmentReportDto report);
    byte[] ExportDepartmentReportToPdf(DepartmentReportDto report);
}
