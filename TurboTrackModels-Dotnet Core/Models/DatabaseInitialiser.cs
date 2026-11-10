using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace TurboTrackModels_Dotnet_Core.Models
{
    /// <summary>
    /// Initialises the database with seed data including roles, admin users, customer profiles, and product catalogs.
    /// Inherits from <see cref="DropCreateDatabaseIfModelChanges{TContext}"/> for development flexibility.
    /// </summary>
    public class DatabaseInitialiser : DropCreateDatabaseIfModelChanges<TTMDbContext> 
    {
        /// <summary>
        /// Seeds the database with initial data.
        /// </summary>
        /// <param name="context">The database context.</param>
        protected override void Seed(TTMDbContext context)
        {
            // create a single UserManager instance to reuse (prevents duplicate local declarations)
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(context));

            //if there are no users, create a default admin user
            if (!context.Users.Any())
            {
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //1. CREATE ALL THE ROLES
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

                //•	Admin will have the responsibility to register new members of staff and provide them with a username (consists of first name, second name and company name for the domain name) and password
                roleManager.Create(new IdentityRole("Admin"));

                //•	Customer – responsible for placing orders, making payments, registering on the website, can viewing order history.
                roleManager.Create(new IdentityRole("Customer"));

                //•	Sales Assistant – responsible for processing customer orders, entering of orders, taking payment, customer registration.
                roleManager.Create(new IdentityRole("Sales Assistant"));

                //•	Invoices Clerk – responsible for sending out invoices to corporate customers and for the processing of payments linked to these orders. 
                roleManager.Create(new IdentityRole("Invoice Clerk"));

                //•	Sales Manager – access to all data
                roleManager.Create(new IdentityRole("Sales Manager"));

                //•	Assistant Manager – same as Sales Assistant plus price amendments and stock control.
                roleManager.Create(new IdentityRole("Assistant Manager"));

                //•	Stock Control Manager- responsible for stock control
                roleManager.Create(new IdentityRole("Stock Control Manager"));

                //•	Warehouse Assistants – responsible for the dispatch of goods to customers.
                roleManager.Create(new IdentityRole("Warehouse Assistant"));

                //•	Delivery Guy – Delivers goods to customers.
                roleManager.Create(new IdentityRole("Delivery Guy"));
                context.SaveChanges();

                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //2. CREATE A DEFAULT ADMIN USER AND ASSIGN IT TO THE ADMIN ROLE
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                if (userManager.FindByName("admin@TTM.com") == null)
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
                    userManager.Create(administrator, "admin123");
                    //assign it to the admin role
                    userManager.AddToRole(administrator.Id, "Admin");
                }
                context.SaveChanges();

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
                if (userManager.FindByName("bill@gmail.com") == null)
                {
                    userManager.Create(bill, "customer1");
                    userManager.AddToRole(bill.Id, "Customer");
                }
                context.SaveChanges();

                //Customer profile for Bill Crow
                var billCustomerProfile = new CustomerProfile
                {
                    Street = "12 Main St",
                    Town = "Glasgow",
                    PostCode = "G1 1AA",
                    PhoneNumber = "07776560983",
                    UserId = bill.Id
                };
                if (!context.Customers.Any(c => c.UserId == bill.Id))
                {
                    context.Customers.Add(billCustomerProfile);
                }
                context.SaveChanges();

                //User account for Sally Crow
                var sally = new User
                {
                    UserName = "sally@gmail.com",
                    Email = "sally@gmail.com",
                    FirstName = "Sally",
                    LastName = "Crow"
                };
                if (userManager.FindByName("sally@gmail.com") == null)
                {
                    userManager.Create(sally, "customer2");
                    userManager.AddToRole(sally.Id, "Customer");
                }
                context.SaveChanges();

                //Customer profile for Sally Crow
                var sallyCustomerProfile = new CustomerProfile
                {
                    Street = "45 High Rd",
                    Town = "Edinburgh",
                    PostCode = "EH2 2BB",
                    PhoneNumber = "01313334444",
                    UserId = sally.Id
                };
                if (!context.Customers.Any(c => c.UserId == sally.Id))
                {
                    context.Customers.Add(sallyCustomerProfile);
                }
                context.SaveChanges();

                //User account for Michael Thompson
                var michael = new User
                {
                    UserName = "michael@gmail.com",
                    Email = "michael@gmail.com",
                    FirstName = "Michael",
                    LastName = "Thompson"
                };
                if (userManager.FindByName("michael@gmail.com") == null)
                {
                    userManager.Create(michael, "customer3");
                    userManager.AddToRole(michael.Id, "Customer");
                }
                context.SaveChanges();

                //Customer profile for Michael Thompson
                var michaelCustomerProfile = new CustomerProfile
                {
                    Street = "78 Queen St",
                    Town = "Aberdeen",
                    PostCode = "AB10 1XX",
                    PhoneNumber = "012245556666",
                    UserId = michael.Id
                };
                if (!context.Customers.Any(c => c.UserId == michael.Id))
                {
                    context.Customers.Add(michaelCustomerProfile);
                }
                context.SaveChanges();

                //User account for Emma Davies
                var emma = new User
                {
                    UserName = "emma@gmail.com",
                    Email = "emma@gmail.com",
                    FirstName = "Emma",
                    LastName = "Davies"
                };
                if (userManager.FindByName("emma@gmail.com") == null)
                {
                    userManager.Create(emma, "customer4");
                    userManager.AddToRole(emma.Id, "Customer");
                }
                context.SaveChanges();

                //Customer profile for Emma Davies
                var emmaCustomerProfile = new CustomerProfile
                {
                    Street = "23 Park Lane",
                    Town = "Stirling",
                    PostCode = "FK8 1ER",
                    PhoneNumber = "017867778888",
                    UserId = emma.Id
                };
                if (!context.Customers.Any(c => c.UserId == emma.Id))
                {
                    context.Customers.Add(emmaCustomerProfile);
                }
                context.SaveChanges();

                //User account for James McKenzie
                var james = new User
                {
                    UserName = "james@gmail.com",
                    Email = "james@gmail.com",
                    FirstName = "James",
                    LastName = "McKenzie"
                };
                if (userManager.FindByName("james@gmail.com") == null)
                {
                    userManager.Create(james, "customer5");
                    userManager.AddToRole(james.Id, "Customer");
                }
                context.SaveChanges();

                //Customer profile for James McKenzie
                var jamesCustomerProfile = new CustomerProfile
                {
                    Street = "56 Elm Avenue",
                    Town = "Dundee",
                    PostCode = "DD1 4QQ",
                    PhoneNumber = "01382 999 0000",
                    UserId = james.Id
                };
                if (!context.Customers.Any(c => c.UserId == james.Id))
                {
                    context.Customers.Add(jamesCustomerProfile);
                }
                context.SaveChanges();

                //User account for Rachel Mitchell
                var rachel = new User
                {
                    UserName = "rachel@gmail.com",
                    Email = "rachel@gmail.com",
                    FirstName = "Rachel",
                    LastName = "Mitchell"
                };
                if (userManager.FindByName("rachel@gmail.com") == null)
                {
                    userManager.Create(rachel, "customer6");
                    userManager.AddToRole(rachel.Id, "Customer");
                }
                context.SaveChanges();

                //Customer profile for Rachel Mitchell
                var rachelCustomerProfile = new CustomerProfile
                {
                    Street = "91 Castle Road",
                    Town = "Inverness",
                    PostCode = "IV2 3QA",
                    PhoneNumber = "01463 111 2222",
                    UserId = rachel.Id
                };
                if (!context.Customers.Any(c => c.UserId == rachel.Id))
                {
                    context.Customers.Add(rachelCustomerProfile);
                }
                context.SaveChanges();

                //User account for David Robertson
                var david = new User
                {
                    UserName = "david@gmail.com",
                    Email = "david@gmail.com",
                    FirstName = "David",
                    LastName = "Robertson"
                };
                if (userManager.FindByName("david@gmail.com") == null)
                {
                    userManager.Create(david, "customer7");
                    userManager.AddToRole(david.Id, "Customer");
                }
                context.SaveChanges();

                //Customer profile for David Robertson
                var davidCustomerProfile = new CustomerProfile
                {
                    Street = "34 Beach Lane",
                    Town = "Ayr",
                    PostCode = "KA7 1HH",
                    PhoneNumber = "01292 333 4444",
                    UserId = david.Id
                };
                if (!context.Customers.Any(c => c.UserId == david.Id))
                {
                    context.Customers.Add(davidCustomerProfile);
                }
                context.SaveChanges();

                //User account for Catherine Stewart
                var catherine = new User
                {
                    UserName = "catherine@gmail.com",
                    Email = "catherine@gmail.com",
                    FirstName = "Catherine",
                    LastName = "Stewart"
                };
                if (userManager.FindByName("catherine@gmail.com") == null)
                {
                    userManager.Create(catherine, "customer8");
                    userManager.AddToRole(catherine.Id, "Customer");
                }
                context.SaveChanges();

                //Customer profile for Catherine Stewart
                var catherineCustomerProfile = new CustomerProfile
                {
                    Street = "67 Valley View",
                    Town = "Falkirk",
                    PostCode = "FK1 5ER",
                    PhoneNumber = "01324 555 6666",
                    UserId = catherine.Id
                };
                if (!context.Customers.Any(c => c.UserId == catherine.Id))
                {
                    context.Customers.Add(catherineCustomerProfile);
                }
                context.SaveChanges();

                //User account for Christopher Hayes
                var christopher = new User
                {
                    UserName = "christopher@gmail.com",
                    Email = "christopher@gmail.com",
                    FirstName = "Christopher",
                    LastName = "Hayes"
                };
                if (userManager.FindByName("christopher@gmail.com") == null)
                {
                    userManager.Create(christopher, "customer9");
                    userManager.AddToRole(christopher.Id, "Customer");
                }
                context.SaveChanges();

                //Customer profile for Christopher Hayes
                var christopherCustomerProfile = new CustomerProfile
                {
                    Street = "15 Riverside Crescent",
                    Town = "Perth",
                    PostCode = "PH1 5HT",
                    PhoneNumber = "01738 777 8888",
                    UserId = christopher.Id
                };
                if (!context.Customers.Any(c => c.UserId == christopher.Id))
                {
                    context.Customers.Add(christopherCustomerProfile);
                }
                context.SaveChanges();


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
                if (userManager.FindByName("kellyJackson@TTM.com") == null)
                {
                    userManager.Create(kelly, "kjAssistant1");
                    userManager.AddToRole(kelly.Id, "Sales Assistant");
                }
                context.SaveChanges();

                //Staff profile for Kelly Jackson
                var kellyStaffProfile = new StaffProfile
                {
                    UserId = kelly.Id
                };
                if (!context.Staff.Any(s => s.UserId == kelly.Id))
                {
                    context.Staff.Add(kellyStaffProfile);
                }
                context.SaveChanges();


                //User account for John Smith - an invoice clerk
                var john = new User
                {
                    UserName = "johnSmith@TTM.com",
                    Email = "johnSmith@TTM.com",
                    FirstName = "John",
                    LastName = "Smith"
                };
                if (userManager.FindByName("johnSmith@TTM.com") == null)
                {
                    userManager.Create(john, "jsClerk1");
                    userManager.AddToRole(john.Id, "Invoice Clerk");
                }
                context.SaveChanges();

                //Staff profile for John Smith
                var johnStaffProfile = new StaffProfile
                {
                    UserId = john.Id
                };
                if (!(context.Staff.Any(s => s.UserId == john.Id)))
                {
                    context.Staff.Add(johnStaffProfile);
                }
                context.SaveChanges();


                //User account for Sarah Lee - a sales manager
                var sarah = new User
                {
                    UserName = "sarahLee@TTM.com",
                    Email = "sarahLee@TTM.com",
                    FirstName = "Sarah",
                    LastName = "Lee"
                };
                if (userManager.FindByName("sarahLee@TTM.com") == null)
                {
                    userManager.Create(sarah, "slManager1");
                    userManager.AddToRole(sarah.Id, "Sales Manager");
                }
                context.SaveChanges();

                //Staff profile for Sarah Lee
                var sarahStaffProfile = new StaffProfile
                {
                    UserId = sarah.Id
                };
                if (!context.Staff.Any(s => s.UserId == sarah.Id))
                {
                    context.Staff.Add(sarahStaffProfile);
                }
                context.SaveChanges();


                //User account for Tom Brown - an assistant manager
                var tom = new User
                {
                    UserName = "tomBrown@TTM.com",
                    Email = "tomBrown@TTM.com",
                    FirstName = "Tom",
                    LastName = "Brown"
                };
                if (userManager.FindByName("tomBrown@TTM.com") == null)
                {
                    userManager.Create(tom, "tbAssistant1");
                    userManager.AddToRole(tom.Id, "Assistant Manager");
                }
                context.SaveChanges();

                //Staff profile for Tom Brown
                var tomStaffProfile = new StaffProfile
                {
                    UserId = tom.Id
                };
                if (!context.Staff.Any(s => s.UserId == tom.Id))
                {
                    context.Staff.Add(tomStaffProfile);
                }
                context.SaveChanges();


                //User account for Lisa Green - a stock control manager
                var lisa = new User
                {
                    UserName = "lisaGreen@TTM.com",
                    Email = "lisaGreen@TTM.com",
                    FirstName = "Lisa",
                    LastName = "Green"
                };
                if (userManager.FindByName("lisaGreen@TTM.com") == null)
                {
                    userManager.Create(lisa, "lgStock1");
                    userManager.AddToRole(lisa.Id, "Stock Control Manager");
                }
                context.SaveChanges();

                //Staff profile for Lisa Green
                var lisaStaffProfile = new StaffProfile
                {
                    UserId = lisa.Id
                };
                if (!(context.Staff.Any(s => s.UserId == lisa.Id)))
                {
                    context.Staff.Add(lisaStaffProfile);
                }
                context.SaveChanges();

                //User account for Mark Wilson - a warehouse assistant
                var mark = new User
                {
                    UserName = "markWilson@TTM.com",
                    Email = "markWilson@TTM.com",
                    FirstName = "Mark",
                    LastName = "Wilson"
                };
                if (userManager.FindByName("markWilson@TTM.com") == null)
                {
                    userManager.Create(mark, "mwWarehouse1");
                    userManager.AddToRole(mark.Id, "Warehouse Assistant");
                }
                context.SaveChanges();

                //Staff profile for Mark Wilson
                var markStaffProfile = new StaffProfile
                {
                    UserId = mark.Id
                };
                if (!context.Staff.Any(s => s.UserId == mark.Id))
                {
                    context.Staff.Add(markStaffProfile);
                }
                context.SaveChanges();

                //User account for David Wilson - a delivery guy
                var davidw = new User
                {
                    UserName = "davidWilson@TTM.com",
                    Email = "davidWilson@TTM.com",
                    FirstName = "David",
                    LastName = "Wilson"
                };
                if (userManager.FindByName("davidWilson@TTM.com") == null)
                {
                    userManager.Create(davidw, "dwDelivery1");
                    userManager.AddToRole(davidw.Id, "Delivery Guy");
                }
                context.SaveChanges();

                //Staff profile for David Wilson
                var davidwStaffProfile = new StaffProfile
                {
                    UserId = davidw.Id
                };
                if (!context.Staff.Any(s => s.UserId == davidw.Id))
                {
                    context.Staff.Add(davidwStaffProfile);
                }
                context.SaveChanges();

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
                if (!context.Products.Any(p => p.ProductName == car1.ProductName))
                {
                    context.Products.Add(car1);
                }
                context.SaveChanges();

                var car2 = new Product
                {
                    ProductId = "C4655",
                    ProductName = "C4655 Aston Martin GT3 Vantage - 12hrs of Bathurst 2025 - Gulf Edition",
                    Description = "The Aston Martin GT3 Vantage storms onto your Scalextric track dressed in motorsport's most revered colours: Gulf blue and orange. Bound for the 2025 Bathurst 12 Hours, this razor-sharp GT3 machine combines the legendary Gulf livery with the Vantage's aggressive stance, creating a model that celebrates endurance racing heritage with contemporary flair.\r\n\r\nFrom its race-ready detailing to its distinctive GT3 profile, this high-detail replica captures the spirit of Mount Panorama's greatest challenge. Piloted by Jamie Day, Mateo Villagomez, and Jaylyn Robotham in the real world, this model brings precision handling and smooth performance to your home circuit, offering collectors and racing enthusiasts a striking piece of modern motorsport history.",
                    Price = 49.99m,
                    StockLevel = 50,
                    ImageUrl = "/Scalextric Car Images/C4655.jpg"
                };
                if (!(context.Products.Any(p => p.ProductName == car2.ProductName)))
                {
                    context.Products.Add(car2);
                }
                context.SaveChanges();

                var car3 = new Product
                {
                    ProductId = "C4479",
                    ProductName = "C4479 FAB-1 - Thunderbirds",
                    Description = "Experience iconic British TV magic with Lady Penelope's FAB 1 from Thunderbirds, now available for the first time as a racing car model. This stunning six-wheeled pink Rolls Royce captures all the luxury and adventure of International Rescue's stylish secret agent, complete with Parker at the wheel and Lady Penelope herself seated regally in the back. From its distinctive grille to its unmistakable profile, this piece of television history brings pure nostalgia racing to your track.\r\n\r\nWhether you're recreating daring rescues or showcasing this eye-catching model in your collection, FAB 1 delivers both theatrical presence and playful appeal. Parents and collectors alike will appreciate the attention to detail in this faithful replica of one of television's most memorable vehicles – guaranteed to be a 'FAB' addition to any racing layout and a conversation starter for Thunderbirds enthusiasts of all ages.",
                    Price = 56.99m,
                    StockLevel = 67,
                    ImageUrl = "/Scalextric Car Images/C4479.jpg"
                };
                if (!context.Products.Any(p => p.ProductName == car3.ProductName))
                {
                    context.Products.Add(car3);
                }
                context.SaveChanges();

                var car4 = new Product
                {
                    ProductId = "C4635",
                    ProductName = "C4635 Mini Miglia - Endaf Owens",
                    Description = "Endaf Owens' bright yellow Mini Miglia brings grassroots motorsport excitement to your Scalextric track, capturing one of the quickest classic racing machines in the UK. This striking #12 racer from the 2024 Mini Miglia Championship combines lightweight character with rev-happy spirit, delivering the intensity and charm that defines Britain's most entertaining one-make series.\r\n\r\nFrom its bold yellow bodywork to its compact, purposeful stance, this model faithfully recreates the look of Owens' 2024 machine with authentic sponsor detailing and race-ready profile. Whether chasing lap records on your home circuit or displayed as a tribute to classic touring car racing, this Mini Miglia delivers grassroots motorsport heritage with undeniable character.",
                    Price = 49.99m,
                    StockLevel = 60,
                    ImageUrl = "/Scalextric Car Images/C4635.jpg"
                };
                if (!context.Products.Any(p => p.ProductName == car4.ProductName))
                {
                    context.Products.Add(car4);
                }
                context.SaveChanges();

                var car5 = new Product
                {
                    ProductId = "C4510",
                    ProductName = "C4510 Brabham BT46 - Niki Lauda Italian GP 1978",
                    Description = "Experience racing history with the iconic Brabham BT46 in its striking red livery, piloted by the legendary Niki Lauda in 1978. This precision-engineered 1:32 scale model captures the revolutionary design of the flat-12 Alfa Romeo-powered machine that carried Lauda during a remarkable chapter in auto racing motorsport lore – a bittersweet moment overshadowed by Ronnie Peterson's tragic accident, yet still a significant achievement in racing history.\r\n\r\nEvery micro detail of this high-fidelity replica has been meticulously recreated, from its distinctive silhouette to its race-day markings. Whether recreating classic racing battles on your mini racing track layout or displaying as part of your collection of championship contenders, this Brabham BT46 delivers authentic racing heritage with the unmistakable presence of a retro golden-era racer.",
                    Price = 56.99m,
                    StockLevel = 9,
                    ImageUrl = "/Scalextric Car Images/C4510.jpg"
                };
                if (!(context.Products.Any(p => p.ProductName == car5.ProductName)))
                {
                    context.Products.Add(car5);
                }
                context.SaveChanges();


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
                    Quantity = 1
                };
                var orderItem2 = new OrderItem
                {
                    ProductId = car2.ProductId,
                    Product = car2,
                    Quantity = 1
                };

                //Attach items to order
                order1.OrderItems.Add(orderItem1);
                order1.OrderItems.Add(orderItem2);

                //NOW calculate total
                order1.TotalCost = order1.OrderItems
                    .Sum(oi => oi.Quantity * oi.Product.Price);

                //Add items to order
                context.Orders.Add(order1);
                context.SaveChanges();


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
                    Quantity = 1
                };
                var orderItem4 = new OrderItem
                {
                    ProductId = car3.ProductId,
                    Product = car3,
                    Quantity = 1
                };
                var orderItem5 = new OrderItem
                {
                    ProductId = car4.ProductId,
                    Product = car4,
                    Quantity = 1
                };

                //Add items to order
                order2.OrderItems.Add(orderItem3);
                order2.OrderItems.Add(orderItem4);
                order2.OrderItems.Add(orderItem5);

                //Calculate total
                order2.TotalCost = order2.OrderItems
                    .Sum(oi => oi.Quantity * oi.Product.Price);

                //Add everything to context
                context.Orders.Add(order2);
                context.SaveChanges();


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
                    Quantity = 1
                };
                var orderItem7 = new OrderItem
                {
                    ProductId = car4.ProductId,
                    Product = car4,
                    Quantity = 1
                };

                //Add items to order
                order3.OrderItems.Add(orderItem6);
                order3.OrderItems.Add(orderItem7);

                //Calculate total
                order3.TotalCost = order3.OrderItems
                    .Sum(oi => oi.Quantity * oi.Product.Price);

                //Add everything to context
                context.Orders.Add(order3);
                context.SaveChanges();
            }
        }
    }
}