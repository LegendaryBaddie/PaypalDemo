import { HttpClient } from '@angular/common/http';
import { Component, Inject } from '@angular/core';
import { Router } from '@angular/router';



@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  private baseUrl: string;
  constructor(
    private router: Router,
    private http: HttpClient,
    @Inject('BASE_URL') baseUrl: string) {
    this.baseUrl = baseUrl}
  public startPurchase(price:number) {
    //get access code from backend
   
    let resp;
    //make a simple get request to backend (just (current address)/paypal)
    this.http.get<string>(this.baseUrl + 'paypal/'+price).subscribe(result => {
      resp = result;
      console.log(resp);
       //naviagate to purchase with the correct price, and access code
      this.router.navigate(['/purchase', { cost: price, token: resp.token.client_token, lifespan: resp.token.expires_in, orderId:resp.orderId}])
    }, error => console.error(error));

   

  };
}


