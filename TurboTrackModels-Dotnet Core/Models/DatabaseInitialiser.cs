using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace TurboTrackModels_Dotnet_Core.Models
{
    /// <summary>
    /// Initialises the database with seed data including roles, admin users, customer profiles, and product catalogs.
    /// </summary>
    public static class DatabaseInitialiser
    {
        public static async Task SeedAsync(
            IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var context = serviceProvider.GetRequiredService<TTMDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            if (!await context.Users.AnyAsync())
            {
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //1. CREATE ALL THE ROLES
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                //•	Admin will have the responsibility to register new members of staff and provide them with a username (consists of first name, second name and company name for the domain name) and password
                await EnsureRoleAsync(roleManager, "Admin");

                //•	Customer – responsible for placing orders, making payments, registering on the website, can viewing order history.
                await EnsureRoleAsync(roleManager, "Customer");

                //•	Sales Assistant – responsible for processing customer orders, entering of orders, taking payment, customer registration.
                await EnsureRoleAsync(roleManager, "Sales Assistant");

                //•	Invoices Clerk – responsible for sending out invoices to corporate customers and for the processing of payments linked to these orders. 
                await EnsureRoleAsync(roleManager, "Invoice Clerk");

                //•	Sales Manager – access to all data
                await EnsureRoleAsync(roleManager, "Sales Manager");

                //•	Assistant Manager – same as Sales Assistant plus price amendments and stock control.
                await EnsureRoleAsync(roleManager, "Assistant Manager");

                //•	Stock Control Manager- responsible for stock control
                await EnsureRoleAsync(roleManager, "Stock Control Manager");

                //•	Warehouse Assistants – responsible for the dispatch of goods to customers.
                await EnsureRoleAsync(roleManager, "Warehouse Assistant");

                //•	Delivery Guy – Delivers goods to customers.
                await EnsureRoleAsync(roleManager, "Delivery Guy");
                await context.SaveChangesAsync();

                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //2. CREATE A DEFAULT ADMIN USER AND ASSIGN IT TO THE ADMIN ROLE
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                if (await userManager.FindByNameAsync("admin@TTM.com") == null)
                {
                    //// Super liberal password validation for password for seeds
                    //userManager.PasswordValidator = new PasswordValidator
                    //{
                    //    RequireDigit = false,
                    //    RequiredLength = 1,
                    //    RequireLowercase = false,
                    //    RequireNonLetterOrDigit = false,
                    //    RequireUppercase = false,
                    //};

                    //create an admin employee
                    var administrator = new User
                    {
                        UserName = "admin@TTM.com",
                        Email = "admin@TTM.com",
                        FirstName = "Adam",
                        LastName = "Adamson",
                        EmailConfirmed = true
                    };
                    //add admin to the Users table
                    EnsureIdentitySuccess(await userManager.CreateAsync(administrator, "Admin123!"));
                    //assign it to the admin role
                    EnsureIdentitySuccess(await userManager.AddToRoleAsync(administrator, "Admin"));
                }
                await context.SaveChangesAsync();

                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //3. CREATE A FEW CUSTOMER USERS AND PROFILES
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                //Create the user accounts for the customers, then create the customer profiles and link it to the user account via the UserId foreign key

                //User account for Bill Crow
                var bill = new User
                {
                    UserName = "bill@gmail.com",
                    Email = "bill@gmail.com",
                    FirstName = "Billy",
                    LastName = "Crow"
                };
                if (await userManager.FindByNameAsync("bill@gmail.com") == null)
                {
                    EnsureIdentitySuccess(await userManager.CreateAsync(bill, "BillCustomer1!"));
                    EnsureIdentitySuccess(await userManager.AddToRoleAsync(bill, "Customer"));
                }
                await context.SaveChangesAsync();

                //Customer profile for Bill Crow
                var billCustomerProfile = new CustomerProfile
                {
                    Street = "12 Main St",
                    Town = "Glasgow",
                    PostCode = "G1 1AA",
                    PhoneNumber = "07776560983",
                    UserId = bill.Id
                };
                if (!await context.Customers.AnyAsync(c => c.UserId == bill.Id))
                {
                    context.Customers.Add(billCustomerProfile);
                }
                await context.SaveChangesAsync();

                //User account for Sally Crow
                var sally = new User
                {
                    UserName = "sally@gmail.com",
                    Email = "sally@gmail.com",
                    FirstName = "Sally",
                    LastName = "Crow"
                };
                if (await userManager.FindByNameAsync("sally@gmail.com") == null)
                {
                    EnsureIdentitySuccess(await userManager.CreateAsync(sally, "SallyCustomer1!"));
                    EnsureIdentitySuccess(await userManager.AddToRoleAsync(sally, "Customer"));
                }
                await context.SaveChangesAsync();

                //Customer profile for Sally Crow
                var sallyCustomerProfile = new CustomerProfile
                {
                    Street = "45 High Rd",
                    Town = "Edinburgh",
                    PostCode = "EH2 2BB",
                    PhoneNumber = "01313334444",
                    UserId = sally.Id
                };
                if (!await context.Customers.AnyAsync(c => c.UserId == sally.Id))
                {
                    context.Customers.Add(sallyCustomerProfile);
                }
                await context.SaveChangesAsync();

                //User account for Michael Thompson
                var michael = new User
                {
                    UserName = "michael@gmail.com",
                    Email = "michael@gmail.com",
                    FirstName = "Michael",
                    LastName = "Thompson"
                };
                if (await userManager.FindByNameAsync("michael@gmail.com") == null)
                {
                    EnsureIdentitySuccess(await userManager.CreateAsync(michael, "MichaelCustomer1!"));
                    EnsureIdentitySuccess(await userManager.AddToRoleAsync(michael, "Customer"));
                }
                await context.SaveChangesAsync();

                //Customer profile for Michael Thompson
                var michaelCustomerProfile = new CustomerProfile
                {
                    Street = "78 Queen St",
                    Town = "Aberdeen",
                    PostCode = "AB10 1XX",
                    PhoneNumber = "012245556666",
                    UserId = michael.Id
                };
                if (!await context.Customers.AnyAsync(c => c.UserId == michael.Id))
                {
                    context.Customers.Add(michaelCustomerProfile);
                }
                await context.SaveChangesAsync();

                //User account for Emma Davies
                var emma = new User
                {
                    UserName = "emma@gmail.com",
                    Email = "emma@gmail.com",
                    FirstName = "Emma",
                    LastName = "Davies"
                };
                if (await userManager.FindByNameAsync("emma@gmail.com") == null)
                {
                    EnsureIdentitySuccess(await userManager.CreateAsync(emma, "EmmaCustomer1!"));
                    EnsureIdentitySuccess(await userManager.AddToRoleAsync(emma, "Customer"));
                }
                await context.SaveChangesAsync();

                //Customer profile for Emma Davies
                var emmaCustomerProfile = new CustomerProfile
                {
                    Street = "23 Park Lane",
                    Town = "Stirling",
                    PostCode = "FK8 1ER",
                    PhoneNumber = "017867778888",
                    UserId = emma.Id
                };
                if (!await context.Customers.AnyAsync(c => c.UserId == emma.Id))
                {
                    context.Customers.Add(emmaCustomerProfile);
                }
                await context.SaveChangesAsync();

                //User account for James McKenzie
                var james = new User
                {
                    UserName = "james@gmail.com",
                    Email = "james@gmail.com",
                    FirstName = "James",
                    LastName = "McKenzie"
                };
                if (await userManager.FindByNameAsync("james@gmail.com") == null)
                {
                    EnsureIdentitySuccess(await userManager.CreateAsync(james, "JamesCustomer1!"));
                    EnsureIdentitySuccess(await userManager.AddToRoleAsync(james, "Customer"));
                }
                await context.SaveChangesAsync();

                //Customer profile for James McKenzie
                var jamesCustomerProfile = new CustomerProfile
                {
                    Street = "56 Elm Avenue",
                    Town = "Dundee",
                    PostCode = "DD1 4QQ",
                    PhoneNumber = "01382 999 0000",
                    UserId = james.Id
                };
                if (!await context.Customers.AnyAsync(c => c.UserId == james.Id))
                {
                    context.Customers.Add(jamesCustomerProfile);
                }
                await context.SaveChangesAsync();

                //User account for Rachel Mitchell
                var rachel = new User
                {
                    UserName = "rachel@gmail.com",
                    Email = "rachel@gmail.com",
                    FirstName = "Rachel",
                    LastName = "Mitchell"
                };
                if (await userManager.FindByNameAsync("rachel@gmail.com") == null)
                {
                    EnsureIdentitySuccess(await userManager.CreateAsync(rachel, "RachelCustomer1!"));
                    EnsureIdentitySuccess(await userManager.AddToRoleAsync(rachel, "Customer"));
                }
                await context.SaveChangesAsync();

                //Customer profile for Rachel Mitchell
                var rachelCustomerProfile = new CustomerProfile
                {
                    Street = "91 Castle Road",
                    Town = "Inverness",
                    PostCode = "IV2 3QA",
                    PhoneNumber = "01463 111 2222",
                    UserId = rachel.Id
                };
                if (!await context.Customers.AnyAsync(c => c.UserId == rachel.Id))
                {
                    context.Customers.Add(rachelCustomerProfile);
                }
                await context.SaveChangesAsync();

                //User account for David Robertson
                var david = new User
                {
                    UserName = "david@gmail.com",
                    Email = "david@gmail.com",
                    FirstName = "David",
                    LastName = "Robertson"
                };
                if (await userManager.FindByNameAsync("david@gmail.com") == null)
                {
                    EnsureIdentitySuccess(await userManager.CreateAsync(david, "DavidCustomer1!"));
                    EnsureIdentitySuccess(await userManager.AddToRoleAsync(david, "Customer"));
                }
                await context.SaveChangesAsync();

                //Customer profile for David Robertson
                var davidCustomerProfile = new CustomerProfile
                {
                    Street = "34 Beach Lane",
                    Town = "Ayr",
                    PostCode = "KA7 1HH",
                    PhoneNumber = "01292 333 4444",
                    UserId = david.Id
                };
                if (!await context.Customers.AnyAsync(c => c.UserId == david.Id))
                {
                    context.Customers.Add(davidCustomerProfile);
                }
                await context.SaveChangesAsync();

                //User account for Catherine Stewart
                var catherine = new User
                {
                    UserName = "catherine@gmail.com",
                    Email = "catherine@gmail.com",
                    FirstName = "Catherine",
                    LastName = "Stewart"
                };
                if (await userManager.FindByNameAsync("catherine@gmail.com") == null)
                {
                    EnsureIdentitySuccess(await userManager.CreateAsync(catherine, "CatherineCustomer1!"));
                    EnsureIdentitySuccess(await userManager.AddToRoleAsync(catherine, "Customer"));
                }
                await context.SaveChangesAsync();

                //Customer profile for Catherine Stewart
                var catherineCustomerProfile = new CustomerProfile
                {
                    Street = "67 Valley View",
                    Town = "Falkirk",
                    PostCode = "FK1 5ER",
                    PhoneNumber = "01324 555 6666",
                    UserId = catherine.Id
                };
                if (!await context.Customers.AnyAsync(c => c.UserId == catherine.Id))
                {
                    context.Customers.Add(catherineCustomerProfile);
                }
                await context.SaveChangesAsync();

                //User account for Christopher Hayes
                var christopher = new User
                {
                    UserName = "christopher@gmail.com",
                    Email = "christopher@gmail.com",
                    FirstName = "Christopher",
                    LastName = "Hayes"
                };
                if (await userManager.FindByNameAsync("christopher@gmail.com") == null)
                {
                    EnsureIdentitySuccess(await userManager.CreateAsync(christopher, "ChrisCustomer1!"));
                    EnsureIdentitySuccess(await userManager.AddToRoleAsync(christopher, "Customer"));
                }
                await context.SaveChangesAsync();

                //Customer profile for Christopher Hayes
                var christopherCustomerProfile = new CustomerProfile
                {
                    Street = "15 Riverside Crescent",
                    Town = "Perth",
                    PostCode = "PH1 5HT",
                    PhoneNumber = "01738 777 8888",
                    UserId = christopher.Id
                };
                if (!await context.Customers.AnyAsync(c => c.UserId == christopher.Id))
                {
                    context.Customers.Add(christopherCustomerProfile);
                }
                await context.SaveChangesAsync();


                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //4.CREATE A FEW STAFF USERS AND PROFILES
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                //User account for Kelly Jackson - a sales assistant
                var kelly = new User
                {
                    UserName = "kellyJackson@TTM.com",
                    Email = "kellyJackson@TTM.com",
                    FirstName = "Kelly",
                    LastName = "Jackson"
                };
                if (await userManager.FindByNameAsync("kellyJackson@TTM.com") == null)
                {
                    EnsureIdentitySuccess(await userManager.CreateAsync(kelly, "KellyAssistant1!"));
                    EnsureIdentitySuccess(await userManager.AddToRoleAsync(kelly, "Sales Assistant"));
                }
                await context.SaveChangesAsync();

                //Staff profile for Kelly Jackson
                var kellyStaffProfile = new StaffProfile
                {
                    UserId = kelly.Id
                };
                if (!await context.Staff.AnyAsync(s => s.UserId == kelly.Id))
                {
                    context.Staff.Add(kellyStaffProfile);
                }
                await context.SaveChangesAsync();


                //User account for John Smith - an invoice clerk
                var john = new User
                {
                    UserName = "johnSmith@TTM.com",
                    Email = "johnSmith@TTM.com",
                    FirstName = "John",
                    LastName = "Smith"
                };
                if (await userManager.FindByNameAsync("johnSmith@TTM.com") == null)
                {
                    EnsureIdentitySuccess(await userManager.CreateAsync(john, "JohnClerk1!"));
                    EnsureIdentitySuccess(await userManager.AddToRoleAsync(john, "Invoice Clerk"));
                }
                await context.SaveChangesAsync();

                //Staff profile for John Smith
                var johnStaffProfile = new StaffProfile
                {
                    UserId = john.Id
                };
                if (!(await context.Staff.AnyAsync(s => s.UserId == john.Id)))
                {
                    context.Staff.Add(johnStaffProfile);
                }
                await context.SaveChangesAsync();


                //User account for Sarah Lee - a sales manager
                var sarah = new User
                {
                    UserName = "sarahLee@TTM.com",
                    Email = "sarahLee@TTM.com",
                    FirstName = "Sarah",
                    LastName = "Lee"
                };
                if (await userManager.FindByNameAsync("sarahLee@TTM.com") == null)
                {
                    EnsureIdentitySuccess(await userManager.CreateAsync(sarah, "SarahManager1!"));
                    EnsureIdentitySuccess(await userManager.AddToRoleAsync(sarah, "Sales Manager"));
                }
                await context.SaveChangesAsync();

                //Staff profile for Sarah Lee
                var sarahStaffProfile = new StaffProfile
                {
                    UserId = sarah.Id
                };
                if (!await context.Staff.AnyAsync(s => s.UserId == sarah.Id))
                {
                    context.Staff.Add(sarahStaffProfile);
                }
                await context.SaveChangesAsync();


                //User account for Tom Brown - an assistant manager
                var tom = new User
                {
                    UserName = "tomBrown@TTM.com",
                    Email = "tomBrown@TTM.com",
                    FirstName = "Tom",
                    LastName = "Brown"
                };
                if (await userManager.FindByNameAsync("tomBrown@TTM.com") == null)
                {
                    EnsureIdentitySuccess(await userManager.CreateAsync(tom, "TomAssistant1!"));
                    EnsureIdentitySuccess(await userManager.AddToRoleAsync(tom, "Assistant Manager"));
                }
                await context.SaveChangesAsync();

                //Staff profile for Tom Brown
                var tomStaffProfile = new StaffProfile
                {
                    UserId = tom.Id
                };
                if (!await context.Staff.AnyAsync(s => s.UserId == tom.Id))
                {
                    context.Staff.Add(tomStaffProfile);
                }
                await context.SaveChangesAsync();


                //User account for Lisa Green - a stock control manager
                var lisa = new User
                {
                    UserName = "lisaGreen@TTM.com",
                    Email = "lisaGreen@TTM.com",
                    FirstName = "Lisa",
                    LastName = "Green"
                };
                if (await userManager.FindByNameAsync("lisaGreen@TTM.com") == null)
                {
                    EnsureIdentitySuccess(await userManager.CreateAsync(lisa, "LisaStock1!"));
                    EnsureIdentitySuccess(await userManager.AddToRoleAsync(lisa, "Stock Control Manager"));
                }
                await context.SaveChangesAsync();

                //Staff profile for Lisa Green
                var lisaStaffProfile = new StaffProfile
                {
                    UserId = lisa.Id
                };
                if (!(await context.Staff.AnyAsync(s => s.UserId == lisa.Id)))
                {
                    context.Staff.Add(lisaStaffProfile);
                }
                await context.SaveChangesAsync();

                //User account for Mark Wilson - a warehouse assistant
                var mark = new User
                {
                    UserName = "markWilson@TTM.com",
                    Email = "markWilson@TTM.com",
                    FirstName = "Mark",
                    LastName = "Wilson"
                };
                if (await userManager.FindByNameAsync("markWilson@TTM.com") == null)
                {
                    EnsureIdentitySuccess(await userManager.CreateAsync(mark, "MarkWarehouse1!"));
                    EnsureIdentitySuccess(await userManager.AddToRoleAsync(mark, "Warehouse Assistant"));
                }
                await context.SaveChangesAsync();

                //Staff profile for Mark Wilson
                var markStaffProfile = new StaffProfile
                {
                    UserId = mark.Id
                };
                if (!await context.Staff.AnyAsync(s => s.UserId == mark.Id))
                {
                    context.Staff.Add(markStaffProfile);
                }
                await context.SaveChangesAsync();

                //User account for David Wilson - a delivery guy
                var davidw = new User
                {
                    UserName = "davidWilson@TTM.com",
                    Email = "davidWilson@TTM.com",
                    FirstName = "David",
                    LastName = "Wilson"
                };
                if (await userManager.FindByNameAsync("davidWilson@TTM.com") == null)
                {
                    EnsureIdentitySuccess(await userManager.CreateAsync(davidw, "DavidDelivery1!"));
                    EnsureIdentitySuccess(await userManager.AddToRoleAsync(davidw, "Delivery Guy"));
                }
                await context.SaveChangesAsync();

                //Staff profile for David Wilson
                var davidwStaffProfile = new StaffProfile
                {
                    UserId = davidw.Id
                };
                if (!await context.Staff.AnyAsync(s => s.UserId == davidw.Id))
                {
                    context.Staff.Add(davidwStaffProfile);
                }
                await context.SaveChangesAsync();

                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //5. CREATE SOME PRODUCTS
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                var car1 = new Product
                {
                    ProductId = "C4300",
                    ProductName = "C4300 Chevrolet Camaro Z28 - Peter John DTM 1982",
                    Description = "Experience the thunderous power of American muscle with the Chevrolet Camaro Z28 in its distinctive white and blue DTM livery, as raced by Peter John in 1982. This high-detail Scalextric model captures every aggressive line of the legendary Z28 that roared through the German touring car championship, bringing European racing history to life with authentic precision and collector-worthy craftsmanship.\r\n\r\nFrom its imposing stance to its faithful recreation of the iconic racing livery, this 1:32 scale Camaro delivers track presence that's impossible to ignore. Whether you're recreating classic touring car battles on any layout or displaying this hot V8 auto beast in your collection, this tire-shredding American racer on European soil promises to deliver the same retro excitement that made these muscle cars the crowd favourites of the DTM's golden era. The clean lines and precise wheel detailing make this racing accessory perfect for navigating every arc of your circuit.",
                    Price = 49.99m,
                    StockLevel = 50,
                    ImageUrl = "/Scalextric Car Images/C4300.jpg"
                };
                if (!await context.Products.AnyAsync(p => p.ProductName == car1.ProductName))
                {
                    context.Products.Add(car1);
                }
                await context.SaveChangesAsync();

                var car2 = new Product
                {
                    ProductId = "C4655",
                    ProductName = "C4655 Aston Martin GT3 Vantage - 12hrs of Bathurst 2025 - Gulf Edition",
                    Description = "The Aston Martin GT3 Vantage storms onto your Scalextric track dressed in motorsport's most revered colours: Gulf blue and orange. Bound for the 2025 Bathurst 12 Hours, this razor-sharp GT3 machine combines the legendary Gulf livery with the Vantage's aggressive stance, creating a model that celebrates endurance racing heritage with contemporary flair.\r\n\r\nFrom its race-ready detailing to its distinctive GT3 profile, this high-detail replica captures the spirit of Mount Panorama's greatest challenge. Piloted by Jamie Day, Mateo Villagomez, and Jaylyn Robotham in the real world, this model brings precision handling and smooth performance to your home circuit, offering collectors and racing enthusiasts a striking piece of modern motorsport history.",
                    Price = 49.99m,
                    StockLevel = 50,
                    ImageUrl = "/Scalextric Car Images/C4655.jpg"
                };
                if (!(await context.Products.AnyAsync(p => p.ProductName == car2.ProductName)))
                {
                    context.Products.Add(car2);
                }
                await context.SaveChangesAsync();

                var car3 = new Product
                {
                    ProductId = "C4479",
                    ProductName = "C4479 FAB-1 - Thunderbirds",
                    Description = "Experience iconic British TV magic with Lady Penelope's FAB 1 from Thunderbirds, now available for the first time as a racing car model. This stunning six-wheeled pink Rolls Royce captures all the luxury and adventure of International Rescue's stylish secret agent, complete with Parker at the wheel and Lady Penelope herself seated regally in the back. From its distinctive grille to its unmistakable profile, this piece of television history brings pure nostalgia racing to your track.\r\n\r\nWhether you're recreating daring rescues or showcasing this eye-catching model in your collection, FAB 1 delivers both theatrical presence and playful appeal. Parents and collectors alike will appreciate the attention to detail in this faithful replica of one of television's most memorable vehicles – guaranteed to be a 'FAB' addition to any racing layout and a conversation starter for Thunderbirds enthusiasts of all ages.",
                    Price = 56.99m,
                    StockLevel = 67,
                    ImageUrl = "/Scalextric Car Images/C4479.jpg"
                };
                if (!await context.Products.AnyAsync(p => p.ProductName == car3.ProductName))
                {
                    context.Products.Add(car3);
                }
                await context.SaveChangesAsync();

                var car4 = new Product
                {
                    ProductId = "C4635",
                    ProductName = "C4635 Mini Miglia - Endaf Owens",
                    Description = "Endaf Owens' bright yellow Mini Miglia brings grassroots motorsport excitement to your Scalextric track, capturing one of the quickest classic racing machines in the UK. This striking #12 racer from the 2024 Mini Miglia Championship combines lightweight character with rev-happy spirit, delivering the intensity and charm that defines Britain's most entertaining one-make series.\r\n\r\nFrom its bold yellow bodywork to its compact, purposeful stance, this model faithfully recreates the look of Owens' 2024 machine with authentic sponsor detailing and race-ready profile. Whether chasing lap records on your home circuit or displayed as a tribute to classic touring car racing, this Mini Miglia delivers grassroots motorsport heritage with undeniable character.",
                    Price = 49.99m,
                    StockLevel = 60,
                    ImageUrl = "/Scalextric Car Images/C4635.jpg"
                };
                if (!await context.Products.AnyAsync(p => p.ProductName == car4.ProductName))
                {
                    context.Products.Add(car4);
                }
                await context.SaveChangesAsync();

                var car5 = new Product
                {
                    ProductId = "C4510",
                    ProductName = "C4510 Brabham BT46 - Niki Lauda Italian GP 1978",
                    Description = "Experience racing history with the iconic Brabham BT46 in its striking red livery, piloted by the legendary Niki Lauda in 1978. This precision-engineered 1:32 scale model captures the revolutionary design of the flat-12 Alfa Romeo-powered machine that carried Lauda during a remarkable chapter in auto racing motorsport lore – a bittersweet moment overshadowed by Ronnie Peterson's tragic accident, yet still a significant achievement in racing history.\r\n\r\nEvery micro detail of this high-fidelity replica has been meticulously recreated, from its distinctive silhouette to its race-day markings. Whether recreating classic racing battles on your mini racing track layout or displaying as part of your collection of championship contenders, this Brabham BT46 delivers authentic racing heritage with the unmistakable presence of a retro golden-era racer.",
                    Price = 56.99m,
                    StockLevel = 9,
                    ImageUrl = "/Scalextric Car Images/C4510.jpg"
                };
                if (!(await context.Products.AnyAsync(p => p.ProductName == car5.ProductName)))
                {
                    context.Products.Add(car5);
                }
                await context.SaveChangesAsync();


                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //6. CREATE SOME ORDERS
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                //Order for Bill Crow - 2 cars
                var order1 = new Order
                {
                    OrderDate = DateTime.Now.AddDays(-10),
                    CustomerID = billCustomerProfile.CustomerID,
                    Customer = billCustomerProfile,
                    OrderItems = new List<OrderItem>(),
                    Status = OrderStatus.Delivered
                };

                //Create order items FIRST
                var orderItem1 = new OrderItem
                {
                    ProductId = car1.ProductId,
                    Product = car1,
                    Quantity = 1,
                    UnitPrice = car1.Price
                };
                var orderItem2 = new OrderItem
                {
                    ProductId = car2.ProductId,
                    Product = car2,
                    Quantity = 1,
                    UnitPrice = car2.Price
                };

                //Attach items to order
                order1.OrderItems.Add(orderItem1);
                order1.OrderItems.Add(orderItem2);

                //NOW calculate total
                order1.TotalCost = order1.OrderItems
                    .Sum(oi => oi.Quantity * oi.UnitPrice);

                //Add items to order
                context.Orders.Add(order1);
                await context.SaveChangesAsync();


                //Order for Sally Crow - 3 cars
                var order2 = new Order
                {
                    OrderDate = DateTime.Now.AddDays(-5),
                    CustomerID = sallyCustomerProfile.CustomerID,
                    Customer = sallyCustomerProfile,
                    OrderItems = new List<OrderItem>(),
                    Status = OrderStatus.Delivered
                };

                //Create order items first
                var orderItem3 = new OrderItem
                {
                    ProductId = car1.ProductId,
                    Product = car1,
                    Quantity = 1,
                    UnitPrice = car1.Price
                };
                var orderItem4 = new OrderItem
                {
                    ProductId = car3.ProductId,
                    Product = car3,
                    Quantity = 1,
                    UnitPrice = car3.Price
                };
                var orderItem5 = new OrderItem
                {
                    ProductId = car4.ProductId,
                    Product = car4,
                    Quantity = 1,
                    UnitPrice = car4.Price
                };

                //Add items to order
                order2.OrderItems.Add(orderItem3);
                order2.OrderItems.Add(orderItem4);
                order2.OrderItems.Add(orderItem5);

                //Calculate total
                order2.TotalCost = order2.OrderItems
                    .Sum(oi => oi.Quantity * oi.UnitPrice);

                //Add everything to context
                context.Orders.Add(order2);
                await context.SaveChangesAsync();


                //Order for Bill Crow - 2 cars
                var order3 = new Order
                {
                    OrderDate = DateTime.Now.AddDays(-2),
                    CustomerID = billCustomerProfile.CustomerID,
                    Customer = billCustomerProfile,
                    OrderItems = new List<OrderItem>(),
                    Status = OrderStatus.Shipped,
                    StaffID = kellyStaffProfile.StaffId
                };

                //Create order items first
                var orderItem6 = new OrderItem
                {
                    ProductId = car5.ProductId,
                    Product = car5,
                    Quantity = 1,
                    UnitPrice = car5.Price
                };
                var orderItem7 = new OrderItem
                {
                    ProductId = car4.ProductId,
                    Product = car4,
                    Quantity = 1,
                    UnitPrice = car4.Price
                };

                //Add items to order
                order3.OrderItems.Add(orderItem6);
                order3.OrderItems.Add(orderItem7);

                //Calculate total
                order3.TotalCost = order3.OrderItems
                    .Sum(oi => oi.Quantity * oi.UnitPrice);

                //Add everything to context
                context.Orders.Add(order3);
                await context.SaveChangesAsync();
            }
        }

        private static void EnsureIdentitySuccess(IdentityResult result)
        {
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(error => $"{error.Code}: {error.Description}"));
                throw new InvalidOperationException($"Database seeding failed: {errors}");
            }
        }

        private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                EnsureIdentitySuccess(await roleManager.CreateAsync(new IdentityRole(roleName)));
            }
        }
    }
}