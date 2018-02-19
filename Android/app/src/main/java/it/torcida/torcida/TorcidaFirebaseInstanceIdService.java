package it.torcida.torcida;

import android.util.Log;

import com.google.firebase.iid.FirebaseInstanceId;
import com.google.firebase.iid.FirebaseInstanceIdService;


public class TorcidaFirebaseInstanceIdService extends FirebaseInstanceIdService {
    @Override
    public void onTokenRefresh(){
        String token = FirebaseInstanceId.getInstance().getToken();
        Log.d(TorcidaFirebaseInstanceIdService.class.getSimpleName(), token);
    }
}
