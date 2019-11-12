<template>
    <div class="container">

        <div  class="row pt-5">
            <div class="col-lg-12 col-md-12 text-center mb-5"> 
                <h4>Register securely to acces your health records using BC Services Card 
                </h4>      
            </div>
            <b-container class="register-with-bc-card">
                <b-row>
                    <b-col fluid> 
                        <img width="350"  src="@/assets/images/landing/002_Devices.png"></b-col>
                    <b-col fluid >
                          <b-button block
                          
                          variant="primary">
                               <img class="mx-1" src= "https://imga.apk.tools/300/f/6/9/ca.bc.gov.id.servicescard.png" height="40">Register for Health Gateway
                             
                          </b-button>

                         </b-col>
                </b-row>
                <div class="col-lg-12 col-md-12 text-left mt-5"> 
    
                     <b-button
                      :class="signupProcess ? 'collapsed' : null"
                     :aria-expanded="signupProcess ? 'true' : 'false'"
                     aria-controls="collapse-4"
                     variant="link"
                     @click="signupProcess = !signupProcess"
                    >
                        I have a BC Services Card, but how do I use it? 
                     </b-button>
                            <b-collapse id="collapse-4" v-model="signupProcess" class="mt-2">
                             <b-card>the pictures of registration process</b-card>
                     </b-collapse>


                          <b-button
                      :class="dongle ? 'collapsed' : null"
                     :aria-expanded="dongle ? 'true' : 'false'"
                     aria-controls="collapse-4"
                     variant="link"
                     @click="dongle = !dongle"
                    >
                        Is there another way? 
                     </b-button>
                            <b-collapse id="collapse-4" v-model="dongle" class="mt-2">
                             <b-card>the pictures of registration process</b-card>
                     </b-collapse>
                <div>
                    
  </div>
                </div>
            </b-container>
        </div>
            </div>
        </div>
    
    </div>
</template>
<script>
  export default {
    data() {
      return {
        signupProcess: false,
        dongle: false,
      }
    }
  }
</script>