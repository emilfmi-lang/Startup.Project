using AutoMapper;
using Startup_Project.Application.Abstractions.UnitOfWork;
using Startup_Project.Application.Dtos.Payment;
using Startup_Project.Application.Interfaces;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Enums;
using Startup_Project.Domain.Enums.Errors;
using Startup_Project.Domain.Exceptions;

namespace Startup_Project.Application.Services.Concretes;

public class PaymentService : IPaymentService
{
    private readonly IGenericRepository<Payment> _paymentRepository;
    private readonly IGenericRepository<Subscription> _subscriptionRepository;
    private readonly IGenericRepository<Package> _packageRepository;
    private readonly IBankService _bankService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PaymentService(
        IGenericRepository<Payment> paymentRepository,
        IGenericRepository<Subscription> subscriptionRepository,
        IGenericRepository<Package> packageRepository,
        IBankService bankService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _paymentRepository = paymentRepository;
        _subscriptionRepository = subscriptionRepository;
        _packageRepository = packageRepository;
        _bankService = bankService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ResponseModel<PaymentResponseDto>> CreatePaymentSessionAsync(Guid userId, PaymentRequestDto dto)
    {
        var package = await _packageRepository.GetByIdAsync(dto.PackageId);
        if (package == null)
            throw NotFoundException.Package(dto.PackageId);

        var bankResponse = await _bankService.CreateSessionAsync(package.Price, package.Name);

        var payment = _mapper.Map<Payment>(dto);

        payment.SetInitialDetails(userId, package.Price, bankResponse.SessionId);

        await _paymentRepository.AddAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        return ResponseModel<PaymentResponseDto>.SuccessResponse(bankResponse);
    }

        public async Task<ResponseModel<bool>> FinalizePaymentAsync(string sessionId)
        {
            var payment = await _paymentRepository.GetSingleAsync(p => p.SessionId == sessionId);
            if (payment == null)
                throw NotFoundException.Payment(sessionId);

            var isActuallyPaid = await _bankService.CheckPaymentStatusAsync(sessionId);
            if (!isActuallyPaid)
                    throw BusinessException.Custom(BusinessErrorCodes.PaymentFailed, "Ödəniş bank tərəfindən təsdiqlənməyib.");

                if (payment.Status == PaymentStatus.Successful)
                    return ResponseModel<bool>.SuccessResponse(true);

                var package = await _packageRepository.GetByIdAsync(payment.PackageId);
                if (package == null)
                    throw NotFoundException.Package(payment.PackageId);

            payment.MarkAsSuccess();

            var subscription = _mapper.Map<Subscription>(package);
            subscription.SetUser(payment.UserId);

            await _subscriptionRepository.AddAsync(subscription);

            var result = await _unitOfWork.SaveChangesAsync();
            if (result <= 0)
                throw BusinessException.Custom(BusinessErrorCodes.UnknownError, "Abunəlik bazaya yazıla bilmədi.");

            return ResponseModel<bool>.SuccessResponse(true);
        }
    }
