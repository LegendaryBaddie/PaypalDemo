import { Component, OnInit, Inject } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import * as $ from "jquery";
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-purchase',
  templateUrl: './purchase.component.html',
  styleUrls: ['./purchase.component.css']
})
export class PurchaseComponent implements OnInit {
  private cost: string;
  private token: string;
  private lifespan: number;
  private tokenType: string;
  private orderId: string;
  private baseUrl: string;
  constructor(private _Activatedroute: ActivatedRoute, private _router: Router, private http: HttpClient,
    @Inject('BASE_URL') baseUrl: string) {
    this.baseUrl = baseUrl;
    _Activatedroute.params.subscribe(result => {
      this.cost = result.cost;
      this.token = result.token;
      this.lifespan = result.lifespan;
      this.tokenType = result.tokenType;
      this.orderId = result.orderId;

    }, error => { console.dir(error) });
    
  }
  
  ngOnInit() {
    //This is one of the most crucial parts of the code
    //Paypal has an api that returns a specific scipt based on parameters in this case "client-id", and "components"
    //client-id is who is recieving the money, in this case it is just sb which the api interprets as sandbox (so it uses the developer ids).
    //angular also does not allow for the <script> element in the component.html, so to work around this we add the element through js.
    //im unaware if there is a npm package for paypal that is offically supported
    this.loadExternalScript("https://www.paypal.com/sdk/js?components=hosted-fields,buttons&client-id=AVZP57SHtLTwirsERs8Slow50abfU2IoU6WnQbFQc_Mqni9UX_W11LHwnTVZZlnZX1fLXv9V46TZlyVt&intent=capture").then(() => {

      var cost = this.cost
      var orderId = this.orderId;
      var http = this.http;
      var baseURL = this.baseUrl;
      //Displays PayPal buttons
      paypal.Buttons({
        commit: false,
        createOrder: function (data, actions) {
          // This function sets up the details of the transaction, including the amount and line item details
        
          var obj = {
            purchase_units: [{
              amount: {
                value: cost
              }
            }]}
          
          console.dir(obj);
          return actions.order.create(obj);
        },
        onCancel: function (data) {
          // Show a cancel page, or return to cart
        },
        onApprove: function (data, actions) {
          // This function captures the funds from the transaction
          return actions.order.capture().then(function (details) {
            // This function shows a transaction success message to your buyer
            alert('Thanks for your purchase!');
          });
        }
      }).render('#paypal-button-container');
      // Eligibility check for advanced credit and debit card payments

      if (paypal.HostedFields.isEligible()) {
        //we need to create an order before they can purchase

        paypal.HostedFields.render({
          createOrder: function () { return orderId; }, // replace order-ID with the order ID
          styles: {
            'input': {
              'font-size': '17px',
              'font-family': 'helvetica, tahoma, calibri, sans-serif',
              'color': '#3a3a3a'
            },
            ':focus': {
              'color': 'black'
            }
          },
          fields: {
            number: {
              selector: '#card-number',
              placeholder: 'card number'
            },
            cvv: {
              selector: '#cvv',
              placeholder: 'card security number'
            },
            expirationDate: {
              selector: '#expiration-date',
              placeholder: 'mm/yy'
            }
          }
        }).then(function (hf) {
          $('#my-sample-form').submit(function (event) {
            event.preventDefault();
            hf.submit({
              // Cardholder Name
              cardholderName: (<HTMLInputElement>document.getElementById('card-holder-name')).value,
              // Billing Address
              billingAddress: {
                streetAddress: (<HTMLInputElement>document.getElementById('card-billing-address-street')).value,      // address_line_1 - street
                extendedAddress: (<HTMLInputElement>document.getElementById('card-billing-address-unit')).value,       // address_line_2 - unit
                region: (<HTMLInputElement>document.getElementById('card-billing-address-state')).value,           // admin_area_1 - state
                locality: (<HTMLInputElement>document.getElementById('card-billing-address-city')).value,          // admin_area_2 - town / city
                postalCode: (<HTMLInputElement>document.getElementById('card-billing-address-zip')).value,           // postal_code - postal_code
                countryCodeAlpha2: "US"   // country_code - country
              }
              // redirect after successful order approval
            }).then(function (e) {
              console.dir(e);
              //finalize the transaction then move to finish page
              http.get<string>(baseURL+'finalize/' + e.orderId).subscribe(result => {
                var resp = result;
                console.log(resp);
                //navigate to purchase with the correct price, and access code
                //this.router.navigate(['/', {}])
              }, error => console.dir(error));
              // make api call to confirm order
              //window.location.replace('https://localhost:44360/');
            }).catch(function (err) {
              console.dir(err);
            });
          });
        });
      }
      else {
        $('#my-sample-form').hide();  // hides the advanced credit and debit card payments fields if merchant isn't eligible
      }
    })
  }

  private loadExternalScript(scriptUrl: string) {

    return new Promise((resolve, reject) => {
      const scriptElement = document.createElement('script');
      scriptElement.src = scriptUrl;
      scriptElement.onload = resolve;

      //this is also an inportant field on the script tag.
      //token is generated from backend api and passed into the page
      scriptElement.setAttribute('data-client-token', this.token);
      document.body.appendChild(scriptElement);
    })
  }
}


