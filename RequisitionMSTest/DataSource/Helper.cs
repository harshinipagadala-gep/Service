using Gep.Cumulus.CSM.Config;
using Gep.Cumulus.CSM.Entities;
using System.Configuration;
using GEP.SMART.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace RequisitionMSTest.DataSource
{
    [ExcludeFromCodeCoverage]
    public class Helper
    {

        private UserExecutionContext _userExecutionContext;
        private static GepConfig _config;

        public static string JWTToken
        {
            get
            {
                var token = "Bearer VdZG9Nd+CMJA6UVBAxPSE5eFdNt3lfJc6CkbmWs1m21CWxlkyCkTkp0*2YbP*U3GnCkleiL8NMI0LpHBA5DUk4hIBhWFXFMClss+VH+ZtasPjeT*9LRU1jj4APVXp60fAUmAft*Y55uHOCPk5jWpgvDqOPIw7cmMUOO1p+Rru9xRljoFtz9F8i7AFxI9WqBgS+4pmufc3wv28FJ1zTMKDp77bE3o8gDoQpAnDHcMdWvFRaTwGYmuVMII9uIPFH7woFw4F5f+H6xDw1c3PYw3goamMAs8cuCVg1oKo1bbc59fO1IZS1LrfJceF1uMrN3q5ek5lQ83eQT9HYjtAd0Dm+B3EDvuNVHFwQar*7GBzdBOhTv233xDRZnvTBlmVut8i4Mlkq6OmMFUa2giG2W1HVtWemJV3pw27hiDyl8z2zh+w5DPpuxkUjTfY2EMHmko8FQ34IM+C*PT47QiPES6kyIe7XJSrRcUhBsWUYDItpzRjf8mX2r7TbHJSi9Z7pvFCHfVr+frF6OKsIgrLa5OCerT0HC4f4rQK4a5BFmn5Z+UtRln1KAu5hRRjro+9UtoRfIZUmJ3KDhMMr+N5Dcf*fB6LFipNVLSBef6m+j3jajtUKmFae8pNQa*mweBU2lUpJlrO9c7FO8LtBcoQMN7AXujbcvvsw2yOUEGwH5xDSSxvmTJVjESzb93+jKR0Pc+2dcW6v26zb13+m9u1pKwqf7K37Ismw8NGAiqWhUw70AW+1srKU6ycO3s0XP9Zk+v7A2DDEa*UyPchHWn**1jOzQ06IHvIjeqv2HOrpTH9IcYJZlI8Nj1GrX7Pg7W6nLkMQSR0nza73rDPmJ1Mor0jOkkaGFTzRkBdP9xGGddMG36Dj8NdAtzYb4gpz+M4IN+wTG1p2Dzd*YifW2PdoWgTPjavQaD4yGTALSdUIr0vyIKMgGsxxAn4xGVHyTnZMNoTn+SjzClPPVl*BxUX1fsPTfnp1pNpxuL7BtFLQxg4QZUCQELT5otGpM0KTMbG1+Oc*TrUgTkJBB6Rto4eqod3weLyAE*dVVDWgjq4uFZdQDrci6n2HeK9BN9qMlrZFBRJMoJ3M6X6OUK4+eCb7eWqIFmG+RYvnqRkNNOuXMBY*bQ03XAxfFoJXNVocV4sanSm9PEa2KUGfGpWxA4ILsr141afUiDI86K6lzE7SHjfabP*+EXQnF5uIChVDGrWib5l8QWDztA4*nWyT29BYDj*INRkfuHO21bIhT074wg*1riCilfw9QsVmU5l3c8Zaij8k7VQOSob8aG2texWu5HNK62Wqdk9Mblb7Kjl9XrVsFlvAOENWFrbqw2uWnJ+cJ1DYoZ*DKiQ7jG*wNeFVAkM364EzrBDZIJfq8Mf5+Mh5uUjjzpbekJHDJWsvUzFOcp1I6u4jPphPbMCg6YoLmhPU+a9fQ*qxd6hvh*LLdna7MJQsMQeYlywhBYQXor4kSwjyGnHqw2pJ4e9CEe+lvi7Vb8t8SMhQL7GXc+QpKTco+MJXKmBKgKHGVDVKyTSqWFThBkTttqnFRiKV+AMJm0*+XGIxHh7w6ZpEvGbEOqxPdHt+3EjE3Ng7Omwpb8s4Yer3fP3zU615JNHEV7CG60o0M+PuBASfrYg2weIRTZZwnPhME9S5PsO0gRH7fR97c6qZHdHAl2v1Ok0BVJjrNmvBx*jyWA0upx*DmSvyNpKmbptA67Wy0b5zdAPE3MzW5pam*sVQ084fs8+n29SPP+MwJ8mrgFNRzqYQMiL9eE7BYJJoQRYswn3STn1YD7z3uUg9dOaKfyo8YLz0U1QwMvtTuIvJEnpO8HfSi9lf92q0LuWTbbrZbR9thUhO*CK5zw23Et6SOgLZSNEHeGj*TGxOKOOYUMotrnK23FCzjEG0u+N1vk7NvKq6Tf68waa58QDuv0n505OjgpYRUhUp0cK2+q4ZIo7xmpiY490sthL6dTDdDI9kGfHTL+9fjuy3OXobcX5FU7nDlnQdV7E8OW7Q*VMsiuiaUnyZfOFcdEXEWtZ4AuNpfgjVO1G9ty7cvNvWj5clxOY5sFnC83PQijkz*bAT7zrq62ASGyPXCqrYvZFfo8KjAdRbcSncrYtImyGijfC5Yh69Hiv5866WQH2bKg7qZomcfsUnK9Hovt1kqadv*MSJCGvl0Xglw6VPx*GG*1Y5iv83SmRm*cNF5+uxpJ0gEcLGmrYbYFzhtVqZLsFL9acYh*7WOEIyAiEYIO7mKJgRZYc+bcaMc2AH12+4Z7MagsKba*GnbD9z6T0y8m9RRyhhnZQEPYR3sWO*ALK43fVW2Hcsw5qNsK2e9d*X0cnU6RKsa+i1PLdTxUk*6n6BpnSdvAiABXDFloSs39Bxf5OqeQHz5LyVgII38US8XhEDEYbIR5JSFtrZ1XBCcmffVVxDlUQHNpwZETOylDpheQTeDDHuzG7CwSi52+rFFIAqdpFcL2dZJcGUwve4yrHTZy1AsST3HURHEUaU2diiwkNQGaKujn2pTAlbunYYJAS2enNVO5Q1BfmwS8bV+iO6nUj19dJQqjtVexIo8RkluG64gqfDLFxelcz79sNnu+1yZdI83RD10oI4qQjSvKhqNZHOOFQQPbUt7F3H3xju0iVKZNX1pQPaBg85z9Yv3Vg2Qh9TyfGxmARImqzWBRT2*8wkKoFs0*8RkFbAvDGSrmvdb5WRs6XmOSI1bXwuktFa1OaoqrpqHNtOldh9QjcF0nIDwNkldb8gKztkZ3fLvg9c4yStnpBYElX4wcloADWHohoMRvKdrx8Fin2qJNpE1dEX7l0FlRTMt7yBPQBny+JRPem5KH*a0tzUWo2HLhR7iM1ebrEfPKmaVuV3JlmBLkEeCzlYU2qfjz8Zr3uaCuNLNW4zKdtXWDDL8VhavrUfw71sOioIn*8XduZ2ldraXJShClP+CVb4ivEFmkUpmfh9p6XXjHwGHQcaq61ZYGk+JPKIgFJkqYbYMsqzoXO1fCT*LYyu4udzjgyiHS9YDBJd0oeWgqaCUZe48Gwl2EfNu10GmxxL6qCWxkTCQ9QmxK8P68NAwGC+wfaJfTkZ97xvBPLHfmP8fHHG4RCea5YL4T4oE3U*qvd1CjlBJ3sHQjk6fkcdGSrRwV+ktx9y9hW1HkEWnlepXcFE0OL9wFYoAVeZeBZ0tTF1EdJy6sTgtvKbeU3OyGKzopSNnNGeNaz+Qnav*6A18knefctC6BXMRQTiT10pzTU7sBHMbbyJgk7EdFIDFIwxJXlbP*37FIbVeTvj6PCIV*NZOR5nul7p22dye1pwtJetYtoOrED3E*SIcKvPHKj4JFrSeTTlUevCEtFRZ+O4qC0dCvwxbPBeqdYdEWeRX*6b*VNovUaIxN9OIWZ9Q1W34YEvQxdfB6avraziecmrsyLygCUJbXUwPj9jVWdR*1Avp5gE9c9j3F+Si7lwS7tb*NtAXXlQD7nvE6WmfoZb7YmkJ3hNPSYaKKEtoNZTnHDHwjlF4TJdb7SdY*RWWIH+8BPFkjNABnepR2nhPZ7nNofdOt3furxr6qHQyTSArLQAUkg0FjJdBARJZVVZ2O21kvT4LXtTrwVw49tpY+Uo1uxb2zNBQwPNHXp9kyzOGiQnVj2DxlWm4RRIFzfDGd*uTje6aAAFR5OsQAFtyUwGcvPYzilJqaxv7ohaVoro5HRNhQABp4469MrCahaR3jpTSVte3wtMgWUnDc72VLTkvouczsU*5yVoNkxLNTRlLdi5Txp3L85g84jXdW0swKIjopbhk*ouilszLc6j3fPoFWkLQLL0B68T0Pi+ZedrSisbGbKcj5tsjVpMVxlRYISuI2jW5SQid4O2Me8a4bETloa948crfeJbwXrIWyWxY0zbu9yNSoNt8K2CWM1uaQFbEGge*AOAVdXLFkBVed2PXPOav*2HnZ6JLKgGL84tQvj62GyFMlko9yl1w9+e8qeUPZXds2RFt9ClWYT49485ryyP8j6hwz1QMbuXb5pOoJyisPirNvHlRlmGpIGHH3PGIxyV0iN8wvFKSiURhgDSIuvW7jE+Bk73jgZhVNrgdPtzW4JFBX4SY0Xo3Mjnk6NCnmMIwMLckXk0mMfuycHsL6hEYlrJrj8ounE7hXOiz0XaHgUpgixnYtj164W4iD+zc6If2*t8zN+t3*DksnSHUjkFOzlzNI*xClRdAQphxbQ+dhQSSCpmvO*TO*LYjb71YOM68BUF6m*SXX6TI3qdrHFNaEvF9c648KjSBPqbTbQri7J7a0RGLUbocwt7pnyB4yy4YQPLRMpOmzYhwwbS4dYyGbCHE4ys*ajzyIIex*VH43qxZrpiJUb8eiE*8LUUUIgpRJG+CiYImyoIvVBeFTm++XURqKsbDWzLLs1Rwgjd9DmYE1qqYUTS1cXJIfN5vcpQs*virC+ct**uYnpraaN0OljEfj6aUA7Xmeb+ayAXZyUS8c6OWAoMNo30E*c3Hm357Vq2FHz+u*8uJxMKMxapn9aW5VAmdSjtDgPZk0MQczGHSKG*C+y75DzmjO03B5cPHHVmZlD0PkC9FO+2na8vy*54m7tSRBfOw4dQlMO7+Yu5ffrsPY+QkULu1f2YicbXL9YO1l13ocUnaepTlHYFTqKnlt8U5LuIE2Im496vsq8ILlBtK2MAoF2nAcu3g8SzPImUWZqVTDi4QV3ZA7Y*r3LrCfgcl5CmZeSjT1VfHiI7OzRpYNDC6rT0ZRvIjpvrG8OC597wryU+rTei4W4EOSETDUXVctWXqdazT5MMblZphfdZy4CfRfO1v3OUNPdW8wiaahJtObKqV8TBG2*3otOYudY8Ws5gLCbPb6FH4rLz0vfmdHS90BJgUUuZjoAW5VvHNxfXkseGhHpgvq6weDOgfiYx+jufuIFO3KYGeo+gkvOMTpBKtq2tVJMBpnVlx+zLmrQLAswqVmS2aZV8833DcNCrhSddtFDVfv+lNKcv9QEizozZ5Bj03uTFGwJw1HJLkMPerlIyYVfKpn75snk2X2G6pvyK2ZMVVuScKqvtCOkvTmIzyuoyn1g6vZFQD*5X0bZwGk+vnsLdnJyCx1iUKic3cqTIaq+PL1d6yIPVMt2MvFitUVfmwaYORYV3g9IngHI4U16MtrY1jsZrUY0NLgTAaZDROz2qKD0+6TBn5lNU1glh*8hnQtvPAe0*P7PTthKhkK2BRH7Ts9yEVqpNtdGVHYEUrWd9LxRRHUIu5MetZg7M1C68FTD9ZGN7GHAvC0xS6LUwNrH2GhJZNJiIUDq0yc7PgxCkwUdBSaXcxXhkmtBvhfXliBvHxAxCoXWuJAQ1xAnGuC77KQf7vKJR0nRc4Uc6zeCgymQqmRX1+57FytugNpA6Ay8nQwsZffkVD+L*k7PnvY*DGWX5sF6Ol7*jJXngiwQPcbrpsr4+lGJ3EVW7peORGVkrqkut42PSTxkWNYajn0lnvcw1ljINMcZ+ayoP+UIrZ9+XHqydQVNH9PfmWnbKdh7urevhTT4wTgcJ79MYJ5UQUWp1HcT5PtJ*udDmPsyq*2gEAbz0k8HxjFnIP*11O7fvaLB+CtcTd7IHTCOnguorm3GcTngOCRBoDzQ6pswkgrN7Qa+2o3*bLYFbzE9kjgaYaCc*qtXkL0gu8rS+1ID8SlaNefmnY+A2PgyjIxSTlpRamnsfK69jw67Gvg2Cn6WxUDrYdpDEDwwLWjhSckd4pwVdwr*IdYZj*e0PMXJefnbkzr74tgPG6d8aG8PktCGYyjwXM8SGkV7mWl6v23i9p3qEcjyQZ+U0IAPi3YGkykJbMBXJmUMHv7Q8btBX23QTZKtIaWYYAgWArevi69*K2gDbsrMWZnlsfN28bWVihUf3DTMA5i7Hy46a5H+ky6h23gaD3iTnuQtbi0oZBVYUsV0nJXjys1+Z3DgNW8wANIY9r5QgGxE0bJTOtPpyaKByVfQ7PEs*p8km+qCWoC7yDGRNqGcW+8DVvdWn7E5i3ZPY0uVQgxgHCxbXHsZI38i3ejFTrvRM973p2tyrWaUu5bZRpp70jfT6*fwX*bUewUFMU9eyX6thNnT+JuhpvReq7RaiW8J1uawYLyTjA2IydOHg4RWzhXnk+kRcxW9DUTJkbJBoRYLmkMYVJPzyWB5y9hkNOJxAskKwgZ55DmIxMMd5Rxhb56a6DxCuLDNYfmby0KhUWeLsLjT2ZTUaM9B7zH8QXyGBuqQmMCHRKXO94hUazqDNxqjEw8LPFcgQ22EP5gFcc5GNXFtBj7IpM9V7ym3mjixbIfA6TcG*0XCS3Ds4FC3+YmQYfGKyn3l55hZ+FnIZXiL7TaUs0Z3Im151QIx59ZYfC7ko2EYx+gPs5wNSd5Rdu190QsLBxxG5yKAtIooAz22mWOrC1sRfHz5yLA87fgwmhsGWlT67djAyY30MpNGln8p9TrASnwLyTx4g+DTwrjXk43vpX0TpAkyRWxXhen3OJdpqgTZtbY0ziR5odpWaVooEh19rMFBipLSzUGSDsZddCxit+xHJMaPmdXBEWfvKvCTZCNQJbBR*lyQ0txxFq0Khu0x62HCnQpCCyAFOirLyNjywT+Ie40FoeRiIEYKE5R45qPN259EE8dijI6dsSIYbccIg1C07D4oHNEZwOgVsTpJ9KDhDIAft2YqnIcELJCS3xKNvmIXckhfQIABKnGfBT5bTBshDlI9VFvE+mhT2tkYuiY1EE3v6CzYlSr0NOTCDvqc*p8QMP83wHc1eu+TrDxQ**tAmyeGUXIRbCz*jCO4sspgq7*j8Mfn7q66insMIGcAz3eRzemCyeBPnuhlycprNWAbrKeYOqS7y*swgJrxCpo19p4DLW7kgfMmVHC5wXukg9Swr9vlEDfYAT*bMeH7LPwUmy+buEle*upZmXFvMycynCQ6BKCiOTIrN5+hplv1finmU3C9sKrXuPmQeXkASQ3avtApySAUwc2G5yufCOJmQZPZfnmpcEomJZN7op6Zhao4kIDWfwclmkHocYP4YcBI0l8xttfELibbDmDShfoe8+s67LL7V2dj3Kq8sNG+e7Ws7K7Mx8yQy*nF9hu8VlQaWI9rZbWqxfUdNRps0rRs8ysqd1UlCZqxDhxfwx+LkQ01RIvps0EgC0b41y1Erb7u4K5Og2o2CYiDfBwh4nHYuJYbeI0bWFKD02ze7ry17v19jv97BGxnY1jYJ9TB6hlEfIUsDHVs4614EfLpGbhBHEDmYhEE9JHKF9N7qZWCiiOFgTGIDQ+YrQx4Mb9IpfiFpv4s0OMIS88EqhxETzC+3E6ik6CUfIQEwNyVjXcwHpU7xLTCRGGyiZC*vdCTS01B1jMieB8LBZ2e+*zFS6QeHOnS05oJ+u6+Ul5vKAVPqH2IeR+IWwLaqfUmsSRgKHhIBWCM*0ZHJZjLa5*6o1xgn5SF+7xevesURw7wfQMnr*CDysCkcmw7LPEpvlcpzdpa52gtRI0HsARvrdRu6LxlYBrH8LsMLSznWPAACk0o319T*YEzS1lFJDwHNTu55sUlYkHWkaMhVPH9ucTMwew*k1tM+VCxPAvCnhIjvBD9qMHvd6Dsp4gxPt+up1NhErIlFBz1p3QlC3g4Nqfmdp+5P1aZTZJbJAwwRM4Gf8fVSUmYxy29x3kXBGzPQBGlQNvaGyZUfaLRNR6+7vInP2CIkL1VGBgdDbLHkmjFX0o4lglHgHnOkQtyNoTjTGzGsnzzMqnVa3*T2493O*1TtE4YrhrHJQrZgBzDNFYA10*XHk0LrkNmwMnvxf5NzzTPbIkYwqcbBATdB6vLP2rK+nIrj5aldtlvRBoQ2x9WiY4M7VH8YhpxbD5iv98zjadLO5r71UPRreMeQPo7Qs0GofinTPlIDH6Vw3gJ*p8Z1jZKhCV2WY4vkgEuWIoTmstpvMk41FjGGD0LZg+wJrFvqbVYnZ3dWqKPbVEBg+otpX9y9njq2C0BoxdD30NRjUutpx7VwVfLxtAwhliBFqtKcTQw4hwfo0LDVYRkiSNH2AUmezAmjoUoERNf8dK+9eLUy3F*Ek2bUfb7VfVQORU+zSFcxqtjay1T*sJrebyjR96pW0yXwEz+rq+WVlM3l0htLfWLqMWE71ehhgzwYHfb1n4dNied7GfmJECkHvxrysAMtxoxy1ciC5j*0WC1*V0qyCaGeF9ZWPPbesuAearEsXElfHch7pk+top413VSYPtbljd1CTRLjmWW6Sy5AgpaC8ihQnRxJ6LCsz+irb8sn9d7BH84OUDz8ZjFbPxnPM9nigpYAPa38lrX*3Uf0ZsD32yni24q4ntCY3p+fxr5OPqiBztsWxi3CwjS+*T3VyRXXNd4l9m0YVxim56k*Y*N7fB2cNq6nrOGcSZ9C8qLoIuVkQbrQ+hOaVu6nTHTGGoozzWhc0NTbdHlJpvrYF8BrzzXiiHv7AL9zzBCjPEhb+RjP7bFuS3QvIYHFX9XF6qy4DK9Tkk+P*3H8wxJs*caP7sqqQI8j83MbW3cmZh3I8CUZxESV1xR8fTjUAdCeEimtB0lK0zMMIFwumXu1QwVCyfNWindg0ENxeHEgwUGcQ1dyiPq5ZpxrGiFIzRIQfFE06VOmW1DI2aO5v1fwrcS1G9njS0aHj*XY19QYUtHFZbJWpdFWM6bCwbyoHronCChfuD65La0GE0M5UMFeYZcZq1dubuN6+4iue0ZeFPIhq*cA352c3voGYfmwzlfZRkjL0BTXDZOpiLCF8cvxlh7SLnYZOH5CgcnVCZMZidEEEY334E+HAqWSo3rO2pZjBbfqy8KmNz3mlRN+iEk7VG1dhQujLQZFwICW0=";
                return token;


            }
        }

        public static void ResetInstance()
        {
            _config = null;
        }

        public static GepConfig GetInstance
        {
            get
            {
                if (_config == null)
                {
                    _config = new GepConfig();
                                        
                    AddValueToConfig("ConfigSqlConn", ConfigurationManager.AppSettings["ConfigSqlConn"]); //@"Data Source=MUM02D2142\SQLEXPRESS;Initial Catalog=Configuration;Integrated Security=False;User Id=sa;Password=Password123;");
                    AddValueToConfig("BuyerSqlConn", ConfigurationManager.AppSettings["BuyerSqlConn"]);//@"Data Source=MUM02D2142\SQLEXPRESS;Initial Catalog=Gep.Cumulus.Buyer;Integrated Security=False;User Id=sa;Password=Password123;");

                    AddValueToConfig("ServiceIP", ConfigurationManager.AppSettings["ServiceIP"]);// "127.0.0.0");
                    AddValueToConfig("ServicePort", ConfigurationManager.AppSettings["ServicePort"]);// "5050");
                    AddValueToConfig("ServiceMexPort", ConfigurationManager.AppSettings["ServiceMexPort"]);// "5051");
                }
                return _config;
            }
        }

        public static GepConfig InitMultiRegion()
        {


            GepConfig _config = new GepConfig();
            MultiRegionConfig.IsCloudService = false;
            var multiregionConfigConnection = ConfigurationManager.AppSettings["ConfigSqlConn"];
            MultiRegionConfig.InitMultiRegionConfig();

            var SmartConfigConn = MultiRegionConfig.GetConfig(CloudConfig.ConfigSqlConn);
            // Reset connection string to configuration database
            AddValueToConfig(_config, GepConfig.ConfigSqlConn, SmartConfigConn);
            AddValueToConfig(_config, GepConfig.BuyerSqlConn, "");
            //GepConfig.SetMinWorkerAndIOCPThreadsInThreadPool(MultiRegionConfig.GetConfig(CloudConfig.MinWorkerThreads), MultiRegionConfig.GetConfig(CloudConfig.MinIOCPThreads));
            GepConfig.SetServicePointManagerConfig(MultiRegionConfig.GetConfig(CloudConfig.UseNagleAlgorithm), MultiRegionConfig.GetConfig(CloudConfig.Expect100Continue), MultiRegionConfig.GetConfig(CloudConfig.DefaultConnectionLimit));
            return _config;
        }

        private static void AddValueToConfig(GepConfig _config, string key, string value)
        {
            if (!_config.ContainsKey(key))
                _config.Add(key, value);
            else
                _config[key] = value ?? string.Empty;
        }


        /// <summary>
        /// Function to Add the Values in the Config Object
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private static void AddValueToConfig(string key, string value)
        {
            if (!_config.ContainsKey(key))
                _config.Add(key, value);
            else
                _config[key] = value ?? string.Empty;
        }

    }
}
