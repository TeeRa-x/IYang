
    [Authorize]
    [Route("partner")]
    public class PartnerController : Controller
    {
        private readonly NewprojectContext dbcontext;
        public PartnerController(NewprojectContext dbcontext)
        {
            this.dbcontext = dbcontext;
        }


        [HttpGet,HttpPost]
        [Route("")]
        public async Task<IActionResult> Partners(PartnerList model)
        {
            if (model == null || model.status == null)
            {
                model = new PartnerList();
                model.status = string.Empty;
            }

            var item = new List<PartnerView>();

            var all = false;
            var have = false;
            var nohave = false;

            if (model.status == "have")
            {
                 all = false;
                have = true;
                 nohave = false;

            }
            else if (model.status == "nohave")
            {
                all = false;
                have = false;
                nohave = true;
            }
            else
            {
                all = true;

            }
            if (all)
            {
                var part = await dbcontext.Partners
                        .Select(x => new PartnerView
                        {
                            IdPartner = x.IdPartner,
                            Name = x.Name,
                            Balance = x.Balance,
                            Status = x.Status
                        }).ToListAsync();
                item.AddRange(part);
            }
            else if (have)
            {
                var part = await dbcontext.Partners
                    .Where(x => x.Status == "ลูกหนี้")
                    .Select(x => new PartnerView
                    {
                        IdPartner = x.IdPartner,
                        Name = x.Name,
                        Balance = x.Balance,
                        Status = x.Status
                    }).ToListAsync();
                item.AddRange(part);
            }
            else if (nohave)
            {
                var part = await dbcontext.Partners
                    .Where(x => x.Status == "เจ้าหนี้")
                    .Select(x => new PartnerView
                    {
                        IdPartner = x.IdPartner,
                        Name = x.Name,
                        Balance = x.Balance,
                        Status = x.Status
                    }).ToListAsync();
                item.AddRange(part);
            }

            model.Items = item;
            return View(model);
        }

        [HttpGet]
        [Route("add")]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [Route("add")]

        public async Task<IActionResult> Add(PartnerView viewModel)
        {
            var pa = new Partner
            {
                IdPartner = Guid.NewGuid(),
                Name = viewModel.Name.Trim(),
                Status = "ไม่มีหนี้",
                Balance = 0
            };
            await dbcontext.Partners.AddAsync(pa);
            await dbcontext.SaveChangesAsync();
            return RedirectToAction("Partners", "Partner");
        }
        [HttpGet]
        [Route("edit")]
        public async Task<IActionResult> Edit(Guid Id)
        {
            var pa = await dbcontext.Partners.FindAsync(Id);
            return View(pa);
        }

        [HttpPost]
        [Route("edit")]
        public async Task<IActionResult> Edit(Partner viewModel)
        {
            var pa = await dbcontext.Partners.FindAsync(viewModel.IdPartner);
            if (pa is not null)
            {
                pa.Name = viewModel.Name;
                await dbcontext.SaveChangesAsync();
            }
            return RedirectToAction("Partners", "Partner");
        }

        [HttpPost]
        [Route("delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var pa = await dbcontext.Partners
                .FindAsync(id);

            if (pa == null)
            return NotFound();

            dbcontext.Partners.Remove(pa);
            await dbcontext.SaveChangesAsync();
            
            return RedirectToAction("Partners", "Partner");
        }

        [HttpGet]
        [Route("detail")]
        public async Task<IActionResult> Detail(Guid id)
        {

            var incomes = await dbcontext.Incomes.Where(x => x.PartnerId == id)
                .Select(x => new ReportItemViewModel
                {
                    TransactionDate = x.DateIncome,
                    Type = "รายรับ",
                    Name = x.Partner.Name,
                    AmountIncome = x.Money,
                    AmountExpense = null,
                    BankName = x.Bank.Bank,
                    AccountName = x.Bank.Name,
                    AccountNo = x.Bank.Banknumber ?? string.Empty,
                    Accounttran = string.Empty,
                    DateUpdate = x.DateUpdate,
                }).ToListAsync();

            var expense = await dbcontext.Expenses.Where(x => x.PartnerId == id)
                .Select(x => new ReportItemViewModel
                {
                    TransactionDate = x.DateExpense,
                    Type = "รายจ่าย",
                    Name = x.Partner.Name,
                    AmountIncome = null,
                    AmountExpense = x.Money,
                    BankName = x.BankAccount.Bank,
                    AccountName = x.BankAccount.Name,
                    AccountNo = x.BankAccount.Banknumber ?? string.Empty,
                    Accounttran = x.Destinationaccount
                ,
                    DateUpdate = x.DateUpdate,
                }).ToListAsync();

            incomes.AddRange(expense);

            var model = new ReportViewModel()
            {
                ReportItems = incomes.OrderBy(x => x.TransactionDate).ToList(),
            };

            return View(model);


        }
        }