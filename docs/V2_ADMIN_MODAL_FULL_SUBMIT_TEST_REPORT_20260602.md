# V2 Admin Modal Full Submit Test Report - 2026-06-02

Base URL: http://localhost:5176
Evidence folder: D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602
Build status: Baseline build passed before run; final build passed after fixes

## Summary

- Pass: 66

## Results

| Trang | Modal | Nút/Action | Test data | Expected | Actual | Status | Evidence |
|---|---|---|---|---|---|---|---|
| FAQ | Thêm FAQ | Đóng bằng nút x | Mở modal rồi bấm x | Modal đóng và có thể mở lại | Modal đã đóng | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\001-FAQ-Th-m-FAQ-open-close-x-2026-06-02T14-09-07-065Z.png |
| FAQ | Thêm FAQ | Đóng bằng nút Hủy/Đóng | Mở modal rồi bấm nút footer | Modal đóng và có thể mở lại | Modal đã đóng | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\002-FAQ-Th-m-FAQ-open-close-cancel-2026-06-02T14-09-07-065Z.png |
| FAQ | Thêm FAQ | Submit modal | Rỗng/thiếu field bắt buộc | Validation/alert/API lỗi rõ ràng, modal không mất dữ liệu | modalOpen=true; apiFailures=0; dialogs=Câu hỏi là bắt buộc! | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\003-FAQ-Th-m-FAQ-before-submit-2026-06-02T14-09-07-065Z.png; D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\003-FAQ-Th-m-FAQ-after-submit-2026-06-02T14-09-07-065Z.png |
| FAQ | Thêm FAQ | Submit modal | Dữ liệu hợp lệ tiếng Việt có dấu | Submit thành công, modal đóng, API không lỗi | modalOpen=false; apiFailures=0; dialogs= | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\004-FAQ-Th-m-FAQ-before-submit-2026-06-02T14-09-07-065Z.png; D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\004-FAQ-Th-m-FAQ-after-submit-2026-06-02T14-09-07-065Z.png |
| Danh mục | Thêm danh mục | Đóng bằng nút x | Mở modal rồi bấm x | Modal đóng và có thể mở lại | Modal đã đóng | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\005-Danh-m-c-Th-m-danh-m-c-open-close-x-2026-06-02T14-09-07-065Z.png |
| Danh mục | Thêm danh mục | Đóng bằng nút Hủy/Đóng | Mở modal rồi bấm nút footer | Modal đóng và có thể mở lại | Modal đã đóng | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\006-Danh-m-c-Th-m-danh-m-c-open-close-cancel-2026-06-02T14-09-07-065Z.png |
| Danh mục | Thêm danh mục | Submit modal | Rỗng/thiếu tên | Validation/alert/API lỗi rõ ràng, modal không mất dữ liệu | modalOpen=true; apiFailures=0; dialogs=Tên danh mục là bắt buộc. | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\007-Danh-m-c-Th-m-danh-m-c-before-submit-2026-06-02T14-09-07-065Z.png; D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\007-Danh-m-c-Th-m-danh-m-c-after-submit-2026-06-02T14-09-07-065Z.png |
| Danh mục | Thêm danh mục | Submit modal | Danh mục cha hợp lệ | Submit thành công, modal đóng, API không lỗi | modalOpen=false; apiFailures=0; dialogs= | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\008-Danh-m-c-Th-m-danh-m-c-before-submit-2026-06-02T14-09-07-065Z.png; D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\008-Danh-m-c-Th-m-danh-m-c-after-submit-2026-06-02T14-09-07-065Z.png |
| Hãng xe | Thêm hãng | Đóng bằng nút x | Mở modal rồi bấm x | Modal đóng và có thể mở lại | Modal đã đóng | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\009-H-ng-xe-Th-m-h-ng-open-close-x-2026-06-02T14-09-07-065Z.png |
| Hãng xe | Thêm hãng | Đóng bằng nút Hủy/Đóng | Mở modal rồi bấm nút footer | Modal đóng và có thể mở lại | Modal đã đóng | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\010-H-ng-xe-Th-m-h-ng-open-close-cancel-2026-06-02T14-09-07-065Z.png |
| Hãng xe | Thêm hãng | Submit modal | Rỗng/thiếu tên | Validation/alert/API lỗi rõ ràng, modal không mất dữ liệu | modalOpen=true; apiFailures=0; dialogs=Tên hãng xe là bắt buộc! | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\011-H-ng-xe-Th-m-h-ng-before-submit-2026-06-02T14-09-07-065Z.png; D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\011-H-ng-xe-Th-m-h-ng-after-submit-2026-06-02T14-09-07-065Z.png |
| Hãng xe | Thêm hãng | Submit modal | Tên hợp lệ + upload logo PNG | Submit thành công, modal đóng, API không lỗi | modalOpen=false; apiFailures=0; dialogs= | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\012-H-ng-xe-Th-m-h-ng-before-submit-2026-06-02T14-09-07-065Z.png; D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\012-H-ng-xe-Th-m-h-ng-after-submit-2026-06-02T14-09-07-065Z.png |
| Hãng xe | Sửa hãng | Submit modal | Đổi/upload logo PNG cho bản ghi đầu tiên | Submit thành công, modal đóng, API không lỗi | modalOpen=false; apiFailures=0; dialogs= | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\013-H-ng-xe-S-a-h-ng-before-submit-2026-06-02T14-09-07-065Z.png; D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\013-H-ng-xe-S-a-h-ng-after-submit-2026-06-02T14-09-07-065Z.png |
| Dòng xe | Thêm dòng xe | Submit modal | Rỗng/thiếu tên và hãng | Validation/alert/API lỗi rõ ràng, modal không mất dữ liệu | modalOpen=true; apiFailures=0; dialogs=Tên dòng xe là bắt buộc! | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\014-D-ng-xe-Th-m-d-ng-xe-before-submit-2026-06-02T14-09-07-065Z.png; D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\014-D-ng-xe-Th-m-d-ng-xe-after-submit-2026-06-02T14-09-07-065Z.png |
| Dòng xe | Thêm dòng xe | Submit modal | Chọn hãng + tên dòng xe hợp lệ | Submit thành công, modal đóng, API không lỗi | modalOpen=false; apiFailures=0; dialogs= | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\015-D-ng-xe-Th-m-d-ng-xe-before-submit-2026-06-02T14-09-07-065Z.png; D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\015-D-ng-xe-Th-m-d-ng-xe-after-submit-2026-06-02T14-09-07-065Z.png |
| Bài viết | Thêm bài viết mới | Mở modal từ Thêm bài viết | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\016-B-i-vi-t-0-generic-open-2026-06-02T14-09-07-065Z.png |
| Bài viết | Thêm bài viết mới | Generic submit | Auto-fill visible fields + submit | Không lỗi API; nếu thiếu nghiệp vụ thì ghi Fail | modalOpen=false; apiFailures=0; dialogs= | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\017-B-i-vi-t-0-generic-submit-2026-06-02T14-09-07-065Z.png |
| Bài viết | Sửa bài viết | Mở modal từ button-1 | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\018-B-i-vi-t-1-generic-open-2026-06-02T14-09-07-065Z.png |
| Bài viết | Sửa bài viết | Generic submit | Auto-fill visible fields + submit | Không lỗi API; nếu thiếu nghiệp vụ thì ghi Fail | modalOpen=false; apiFailures=0; dialogs= | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\019-B-i-vi-t-1-generic-submit-2026-06-02T14-09-07-065Z.png |
| Bài viết | Sửa bài viết | Mở modal từ button-2 | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\020-B-i-vi-t-2-generic-open-2026-06-02T14-09-07-065Z.png |
| Bài viết | Sửa bài viết | Generic submit | Auto-fill visible fields + submit | Không lỗi API; nếu thiếu nghiệp vụ thì ghi Fail | modalOpen=false; apiFailures=0; dialogs= | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\021-B-i-vi-t-2-generic-submit-2026-06-02T14-09-07-065Z.png |
| Banner | Thêm banner mới | Mở modal từ Thêm banner | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\022-Banner-0-generic-open-2026-06-02T14-09-07-065Z.png |
| Banner | Thêm banner mới | Generic submit | Auto-fill visible fields + submit | Không lỗi API; nếu thiếu nghiệp vụ thì ghi Fail | modalOpen=false; apiFailures=0; dialogs= | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\023-Banner-0-generic-submit-2026-06-02T14-09-07-065Z.png |
| Banner | Sửa banner | Mở modal từ button-1 | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\024-Banner-1-generic-open-2026-06-02T14-09-07-065Z.png |
| Banner | Sửa banner | Generic submit | Auto-fill visible fields + submit | Không lỗi API; nếu thiếu nghiệp vụ thì ghi Fail | modalOpen=false; apiFailures=0; dialogs= | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\025-Banner-1-generic-submit-2026-06-02T14-09-07-065Z.png |
| Banner | Sửa banner | Mở modal từ button-2 | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\026-Banner-2-generic-open-2026-06-02T14-09-07-065Z.png |
| Banner | Sửa banner | Generic submit | Auto-fill visible fields + submit | Không lỗi API; nếu thiếu nghiệp vụ thì ghi Fail | modalOpen=false; apiFailures=0; dialogs= | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\027-Banner-2-generic-submit-2026-06-02T14-09-07-065Z.png |
| Liên hệ | Chi tiết yêu cầu liên hệ | Mở modal từ button-0 | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\028-Li-n-h--0-generic-open-2026-06-02T14-09-07-065Z.png |
| Liên hệ | Chi tiết yêu cầu liên hệ | Mở modal từ button-1 | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\029-Li-n-h--1-generic-open-2026-06-02T14-09-07-065Z.png |
| Voucher | Thêm Voucher | Mở modal từ Thêm Voucher | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\030-Voucher-0-generic-open-2026-06-02T14-09-07-065Z.png |
| Voucher | Thêm Voucher | Generic submit | Auto-fill visible fields + submit | Không lỗi API; nếu thiếu nghiệp vụ thì ghi Fail | modalOpen=false; apiFailures=0; dialogs= | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\031-Voucher-0-generic-submit-2026-06-02T14-09-07-065Z.png |
| Voucher | Sửa Voucher | Mở modal từ button-1 | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\032-Voucher-1-generic-open-2026-06-02T14-09-07-065Z.png |
| Voucher | Sửa Voucher | Generic submit | Auto-fill visible fields + submit | Không lỗi API; nếu thiếu nghiệp vụ thì ghi Fail | modalOpen=false; apiFailures=0; dialogs= | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\033-Voucher-1-generic-submit-2026-06-02T14-09-07-065Z.png |
| Voucher | Sửa Voucher | Mở modal từ button-2 | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\034-Voucher-2-generic-open-2026-06-02T14-09-07-065Z.png |
| Voucher | Sửa Voucher | Generic submit | Auto-fill visible fields + submit | Không lỗi API; nếu thiếu nghiệp vụ thì ghi Fail | modalOpen=false; apiFailures=0; dialogs= | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\035-Voucher-2-generic-submit-2026-06-02T14-09-07-065Z.png |
| Xe máy | Thêm sản phẩm mới | Mở modal từ Thêm xe máy | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\036-Xe-m-y-0-generic-open-2026-06-02T14-09-07-065Z.png |
| Xe máy | Thêm sản phẩm mới | Generic submit | Auto-fill visible fields + submit | Không lỗi API; nếu thiếu nghiệp vụ thì ghi Fail | modalOpen=false; apiFailures=0; dialogs= | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\037-Xe-m-y-0-generic-submit-2026-06-02T14-09-07-065Z.png |
| Xe máy | Sửa sản phẩm | Mở modal từ button-2 | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\038-Xe-m-y-2-generic-open-2026-06-02T14-09-07-065Z.png |
| Xe máy | Sửa sản phẩm | Generic submit | Auto-fill visible fields + submit | Không lỗi API; nếu thiếu nghiệp vụ thì ghi Fail | modalOpen=false; apiFailures=0; dialogs= | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\039-Xe-m-y-2-generic-submit-2026-06-02T14-09-07-065Z.png |
| Phụ tùng | Thêm sản phẩm mới | Mở modal từ Thêm phụ tùng | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\040-Ph--t-ng-0-generic-open-2026-06-02T14-09-07-065Z.png |
| Phụ tùng | Thêm sản phẩm mới | Generic submit | Auto-fill visible fields + submit | Không lỗi API; nếu thiếu nghiệp vụ thì ghi Fail | modalOpen=false; apiFailures=0; dialogs= | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\041-Ph--t-ng-0-generic-submit-2026-06-02T14-09-07-065Z.png |
| Phụ tùng | Sửa sản phẩm | Mở modal từ button-2 | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\042-Ph--t-ng-2-generic-open-2026-06-02T14-09-07-065Z.png |
| Phụ tùng | Sửa sản phẩm | Generic submit | Auto-fill visible fields + submit | Không lỗi API; nếu thiếu nghiệp vụ thì ghi Fail | modalOpen=false; apiFailures=0; dialogs= | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\043-Ph--t-ng-2-generic-submit-2026-06-02T14-09-07-065Z.png |
| Tồn kho | Chi tiết giữ chỗ | Mở modal từ button-0 | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\044-T-n-kho-0-generic-open-2026-06-02T14-09-07-065Z.png |
| Tồn kho | Cập nhật ngưỡng tồn thấp | Mở modal từ button-1 | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\045-T-n-kho-1-generic-open-2026-06-02T14-09-07-065Z.png |
| Tồn kho | Cập nhật ngưỡng tồn thấp | Generic submit | Auto-fill visible fields + submit | Không lỗi API; nếu thiếu nghiệp vụ thì ghi Fail | modalOpen=false; apiFailures=0; dialogs= | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\046-T-n-kho-1-generic-submit-2026-06-02T14-09-07-065Z.png |
| Tồn kho | Nhập/Xuất/Điều chỉnh tồn | Mở modal từ button-2 | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\047-T-n-kho-2-generic-open-2026-06-02T14-09-07-065Z.png |
| Tồn kho | Nhập/Xuất/Điều chỉnh tồn | Generic submit | Auto-fill visible fields + submit | Không lỗi API; nếu thiếu nghiệp vụ thì ghi Fail | modalOpen=false; apiFailures=0; dialogs=Xác nhận nhập kho? Tồn sau thay đổi: 18 | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\048-T-n-kho-2-generic-submit-2026-06-02T14-09-07-065Z.png |
| Phiếu kho | Tạo phiếu kho | Mở modal từ Tạo phiếu kho | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\049-Phi-u-kho-0-generic-open-2026-06-02T14-09-07-065Z.png |
| Phiếu kho | Tạo phiếu kho | Generic submit | Auto-fill visible fields + submit | Không lỗi API; nếu thiếu nghiệp vụ thì ghi Fail | modalOpen=false; apiFailures=0; dialogs= | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\050-Phi-u-kho-0-generic-submit-2026-06-02T14-09-07-065Z.png |
| Phiếu kho | Chi tiết phiếu kho - PK120260602141216 | Mở modal từ button-1 | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\051-Phi-u-kho-1-generic-open-2026-06-02T14-09-07-065Z.png |
| Phiếu kho | Chi tiết phiếu kho - PK120260602140437 | Mở modal từ button-2 | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\052-Phi-u-kho-2-generic-open-2026-06-02T14-09-07-065Z.png |
| Khách hàng | Hồ sơ khách hàng 360 | Mở modal từ button-0 | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\053-Kh-ch-h-ng-0-generic-open-2026-06-02T14-09-07-065Z.png |
| Khách hàng | Ghi chú chăm sóc - Khach La | Mở modal từ button-1 | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\054-Kh-ch-h-ng-1-generic-open-2026-06-02T14-09-07-065Z.png |
| Khách hàng | Ghi chú chăm sóc - Khach La | Generic submit | Auto-fill visible fields + submit | Không lỗi API; nếu thiếu nghiệp vụ thì ghi Fail | modalOpen=false; apiFailures=0; dialogs= | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\055-Kh-ch-h-ng-1-generic-submit-2026-06-02T14-09-07-065Z.png |
| Khách hàng | Hồ sơ khách hàng 360 | Mở modal từ button-2 | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\056-Kh-ch-h-ng-2-generic-open-2026-06-02T14-09-07-065Z.png |
| Bảo hành | Tạo phiếu bảo hành | Mở modal từ Tạo phiếu bảo hành | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\057-B-o-h-nh-0-generic-open-2026-06-02T14-09-07-065Z.png |
| Bảo hành | Tạo phiếu bảo hành | Generic submit | Auto-fill visible fields + submit | Không lỗi API; nếu thiếu nghiệp vụ thì ghi Fail | modalOpen=false; apiFailures=0; dialogs= | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\058-B-o-h-nh-0-generic-submit-2026-06-02T14-09-07-065Z.png |
| Bảo hành | Chi tiết bảo hành - BH20260602141336512 | Mở modal từ button-1 | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\059-B-o-h-nh-1-generic-open-2026-06-02T14-09-07-065Z.png |
| Bảo hành | Chi tiết bảo hành - BH20260602141336512 | Generic submit | Auto-fill visible fields + submit | Không lỗi API; nếu thiếu nghiệp vụ thì ghi Fail | modalOpen=true; apiFailures=0; dialogs= | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\060-B-o-h-nh-1-generic-submit-2026-06-02T14-09-07-065Z.png |
| Bảo hành | Chi tiết bảo hành - BH20260602140558014 | Mở modal từ button-2 | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\061-B-o-h-nh-2-generic-open-2026-06-02T14-09-07-065Z.png |
| Bảo hành | Chi tiết bảo hành - BH20260602140558014 | Generic submit | Auto-fill visible fields + submit | Không lỗi API; nếu thiếu nghiệp vụ thì ghi Fail | modalOpen=true; apiFailures=0; dialogs= | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\062-B-o-h-nh-2-generic-submit-2026-06-02T14-09-07-065Z.png |
| Vận hành nâng cao | Tạo phiếu trả hàng | Mở modal từ Tạo phiếu trả hàng | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\063-V-n-h-nh-n-ng-cao-0-generic-open-2026-06-02T14-09-07-065Z.png |
| Vận hành nâng cao | Tạo phiếu trả hàng | Generic submit | Auto-fill visible fields + submit | Không lỗi API; nếu thiếu nghiệp vụ thì ghi Fail | modalOpen=false; apiFailures=0; dialogs= | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\064-V-n-h-nh-n-ng-cao-0-generic-submit-2026-06-02T14-09-07-065Z.png |
| Nghiệp vụ cửa hàng | Nhập thông tin nghiệp vụ | Mở modal từ Tạo mới | Generic open/visual | Modal mở, không tràn rõ ràng | Modal hiển thị | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\065-Nghi-p-v--c-a-h-ng-1-generic-open-2026-06-02T14-09-07-065Z.png |
| Nghiệp vụ cửa hàng | Nhập thông tin nghiệp vụ | Generic submit | Auto-fill visible fields + submit | Không lỗi API; nếu thiếu nghiệp vụ thì ghi Fail | modalOpen=false; apiFailures=0; dialogs= | Pass | D:\MotorTeam\MoToSale-End\docs\modal-full-submit-test-20260602\066-Nghi-p-v--c-a-h-ng-1-generic-submit-2026-06-02T14-09-07-065Z.png |

## API/Dialog Evidence

### 1. FAQ - Thêm FAQ - Đóng bằng nút x

API responses:
- GET 200 http://localhost:5176/api/content/faq?page=1&pageSize=10 => {"items":[{"id":1,"question":"Cửa hàng có hỗ trợ trả góp không?","answer":"Có. Nhân viên sẽ tư vấn hồ sơ và phương án phù hợp tại showroom.","category":"Thanh toán","sortOrder":1,"status":1},{"id":5,"question":"Modal test FAQ có dấu 1780408559612","answer":"Câu trả lời kiểm thử modal có dấu, không bị lỗi encoding.","category":"Kiểm thử","sortOrder"
- GET 200 http://localhost:5176/api/content/faq?page=1&pageSize=10 => {"items":[{"id":1,"question":"Cửa hàng có hỗ trợ trả góp không?","answer":"Có. Nhân viên sẽ tư vấn hồ sơ và phương án phù hợp tại showroom.","category":"Thanh toán","sortOrder":1,"status":1},{"id":5,"question":"Modal test FAQ có dấu 1780408559612","answer":"Câu trả lời kiểm thử modal có dấu, không bị lỗi encoding.","category":"Kiểm thử","sortOrder"

### 2. FAQ - Thêm FAQ - Đóng bằng nút Hủy/Đóng

API responses:
- GET 200 http://localhost:5176/api/content/faq?page=1&pageSize=10 => {"items":[{"id":1,"question":"Cửa hàng có hỗ trợ trả góp không?","answer":"Có. Nhân viên sẽ tư vấn hồ sơ và phương án phù hợp tại showroom.","category":"Thanh toán","sortOrder":1,"status":1},{"id":5,"question":"Modal test FAQ có dấu 1780408559612","answer":"Câu trả lời kiểm thử modal có dấu, không bị lỗi encoding.","category":"Kiểm thử","sortOrder"
- GET 200 http://localhost:5176/api/content/faq?page=1&pageSize=10 => {"items":[{"id":1,"question":"Cửa hàng có hỗ trợ trả góp không?","answer":"Có. Nhân viên sẽ tư vấn hồ sơ và phương án phù hợp tại showroom.","category":"Thanh toán","sortOrder":1,"status":1},{"id":5,"question":"Modal test FAQ có dấu 1780408559612","answer":"Câu trả lời kiểm thử modal có dấu, không bị lỗi encoding.","category":"Kiểm thử","sortOrder"

### 3. FAQ - Thêm FAQ - Submit modal

Dialogs:
- alert: Câu hỏi là bắt buộc!

### 4. FAQ - Thêm FAQ - Submit modal

API responses:
- POST 200 http://localhost:5176/api/content/faq => {"id":7}
- GET 200 http://localhost:5176/api/content/faq?page=1&pageSize=10 => {"items":[{"id":1,"question":"Cửa hàng có hỗ trợ trả góp không?","answer":"Có. Nhân viên sẽ tư vấn hồ sơ và phương án phù hợp tại showroom.","category":"Thanh toán","sortOrder":1,"status":1},{"id":5,"question":"Modal test FAQ có dấu 1780408559612","answer":"Câu trả lời kiểm thử modal có dấu, không bị lỗi encoding.","category":"Kiểm thử","sortOrder"

### 5. Danh mục - Thêm danh mục - Đóng bằng nút x

API responses:
- GET 200 http://localhost:5176/api/categories?activeOnly=false => {"items":[{"id":6,"parentId":2,"name":"Dầu nhớt","slug":"dau-nhot","kind":2,"sortOrder":1,"status":1},{"id":1,"parentId":null,"name":"Xe máy","slug":"xe-may","kind":1,"sortOrder":1,"status":1},{"id":3,"parentId":1,"name":"Xe tay ga","slug":"xe-tay-ga","kind":1,"sortOrder":1,"status":1},{"id":7,"parentId":2,"name":"Lốp xe","slug":"lop-xe","kind":2,"
- GET 200 http://localhost:5176/api/categories?activeOnly=false => {"items":[{"id":6,"parentId":2,"name":"Dầu nhớt","slug":"dau-nhot","kind":2,"sortOrder":1,"status":1},{"id":1,"parentId":null,"name":"Xe máy","slug":"xe-may","kind":1,"sortOrder":1,"status":1},{"id":3,"parentId":1,"name":"Xe tay ga","slug":"xe-tay-ga","kind":1,"sortOrder":1,"status":1},{"id":7,"parentId":2,"name":"Lốp xe","slug":"lop-xe","kind":2,"

### 6. Danh mục - Thêm danh mục - Đóng bằng nút Hủy/Đóng

API responses:
- GET 200 http://localhost:5176/api/categories?activeOnly=false => {"items":[{"id":6,"parentId":2,"name":"Dầu nhớt","slug":"dau-nhot","kind":2,"sortOrder":1,"status":1},{"id":1,"parentId":null,"name":"Xe máy","slug":"xe-may","kind":1,"sortOrder":1,"status":1},{"id":3,"parentId":1,"name":"Xe tay ga","slug":"xe-tay-ga","kind":1,"sortOrder":1,"status":1},{"id":7,"parentId":2,"name":"Lốp xe","slug":"lop-xe","kind":2,"
- GET 200 http://localhost:5176/api/categories?activeOnly=false => {"items":[{"id":6,"parentId":2,"name":"Dầu nhớt","slug":"dau-nhot","kind":2,"sortOrder":1,"status":1},{"id":1,"parentId":null,"name":"Xe máy","slug":"xe-may","kind":1,"sortOrder":1,"status":1},{"id":3,"parentId":1,"name":"Xe tay ga","slug":"xe-tay-ga","kind":1,"sortOrder":1,"status":1},{"id":7,"parentId":2,"name":"Lốp xe","slug":"lop-xe","kind":2,"

### 7. Danh mục - Thêm danh mục - Submit modal

Dialogs:
- alert: Tên danh mục là bắt buộc.

### 8. Danh mục - Thêm danh mục - Submit modal

API responses:
- POST 200 http://localhost:5176/api/categories => {"id":14}
- GET 200 http://localhost:5176/api/categories?activeOnly=false => {"items":[{"id":6,"parentId":2,"name":"Dầu nhớt","slug":"dau-nhot","kind":2,"sortOrder":1,"status":1},{"id":1,"parentId":null,"name":"Xe máy","slug":"xe-may","kind":1,"sortOrder":1,"status":1},{"id":3,"parentId":1,"name":"Xe tay ga","slug":"xe-tay-ga","kind":1,"sortOrder":1,"status":1},{"id":7,"parentId":2,"name":"Lốp xe","slug":"lop-xe","kind":2,"

### 9. Hãng xe - Thêm hãng - Đóng bằng nút x

API responses:
- GET 200 http://localhost:5176/api/brands?page=1&pageSize=20 => {"items":[{"id":1,"name":"Honda","slug":"honda","logoUrl":"/uploads/brands/9ebb8244d6244946a84e1faedd52b70a.png","status":1},{"id":9,"name":"Modal Brand 1780407655062","slug":"modal-brand-1780407655062","logoUrl":"/uploads/brands/7c2d4fdfa665465cb8d89f69ad326ea3.png","status":1},{"id":10,"name":"Modal Brand 1780408133393","slug":"modal-brand-178040
- GET 200 http://localhost:5176/api/brands?page=1&pageSize=20 => {"items":[{"id":1,"name":"Honda","slug":"honda","logoUrl":"/uploads/brands/9ebb8244d6244946a84e1faedd52b70a.png","status":1},{"id":9,"name":"Modal Brand 1780407655062","slug":"modal-brand-1780407655062","logoUrl":"/uploads/brands/7c2d4fdfa665465cb8d89f69ad326ea3.png","status":1},{"id":10,"name":"Modal Brand 1780408133393","slug":"modal-brand-178040

### 10. Hãng xe - Thêm hãng - Đóng bằng nút Hủy/Đóng

API responses:
- GET 200 http://localhost:5176/api/brands?page=1&pageSize=20 => {"items":[{"id":1,"name":"Honda","slug":"honda","logoUrl":"/uploads/brands/9ebb8244d6244946a84e1faedd52b70a.png","status":1},{"id":9,"name":"Modal Brand 1780407655062","slug":"modal-brand-1780407655062","logoUrl":"/uploads/brands/7c2d4fdfa665465cb8d89f69ad326ea3.png","status":1},{"id":10,"name":"Modal Brand 1780408133393","slug":"modal-brand-178040
- GET 200 http://localhost:5176/api/brands?page=1&pageSize=20 => {"items":[{"id":1,"name":"Honda","slug":"honda","logoUrl":"/uploads/brands/9ebb8244d6244946a84e1faedd52b70a.png","status":1},{"id":9,"name":"Modal Brand 1780407655062","slug":"modal-brand-1780407655062","logoUrl":"/uploads/brands/7c2d4fdfa665465cb8d89f69ad326ea3.png","status":1},{"id":10,"name":"Modal Brand 1780408133393","slug":"modal-brand-178040

### 11. Hãng xe - Thêm hãng - Submit modal

Dialogs:
- alert: Tên hãng xe là bắt buộc!

### 12. Hãng xe - Thêm hãng - Submit modal

API responses:
- POST 200 http://localhost:5176/api/brands => {"id":13}
- POST 200 http://localhost:5176/api/brands/13/logo => {"url":"/uploads/brands/640f55470d394951a8a6300dd55941e4.png"}
- GET 200 http://localhost:5176/api/brands?page=1&pageSize=20 => {"items":[{"id":1,"name":"Honda","slug":"honda","logoUrl":"/uploads/brands/9ebb8244d6244946a84e1faedd52b70a.png","status":1},{"id":9,"name":"Modal Brand 1780407655062","slug":"modal-brand-1780407655062","logoUrl":"/uploads/brands/7c2d4fdfa665465cb8d89f69ad326ea3.png","status":1},{"id":10,"name":"Modal Brand 1780408133393","slug":"modal-brand-178040

### 13. Hãng xe - Sửa hãng - Submit modal

API responses:
- PUT 200 http://localhost:5176/api/brands/1 => {"id":1}
- POST 200 http://localhost:5176/api/brands/1/logo => {"url":"/uploads/brands/67aa31e1831647bdb32948dee94ffb95.png"}
- GET 200 http://localhost:5176/api/brands?page=1&pageSize=20 => {"items":[{"id":1,"name":"Honda","slug":"honda","logoUrl":"/uploads/brands/67aa31e1831647bdb32948dee94ffb95.png","status":1},{"id":9,"name":"Modal Brand 1780407655062","slug":"modal-brand-1780407655062","logoUrl":"/uploads/brands/7c2d4fdfa665465cb8d89f69ad326ea3.png","status":1},{"id":10,"name":"Modal Brand 1780408133393","slug":"modal-brand-178040

### 14. Dòng xe - Thêm dòng xe - Submit modal

Dialogs:
- alert: Tên dòng xe là bắt buộc!

### 15. Dòng xe - Thêm dòng xe - Submit modal

API responses:
- POST 200 http://localhost:5176/api/models => {"id":16}
- GET 200 http://localhost:5176/api/models?page=1&pageSize=20 => {"items":[{"id":4,"brandId":1,"name":"Air Blade","slug":"honda-air-blade","status":1},{"id":10,"brandId":5,"name":"Elegant","slug":"sym-elegant","status":1},{"id":3,"brandId":2,"name":"Exciter","slug":"yamaha-exciter","status":1},{"id":7,"brandId":2,"name":"Grande","slug":"yamaha-grande","status":1},{"id":6,"brandId":2,"name":"Janus","slug":"yamaha

### 16. Bài viết - Thêm bài viết mới - Mở modal từ Thêm bài viết

- Không có dialog/API liên quan được capture.

### 17. Bài viết - Thêm bài viết mới - Generic submit

API responses:
- POST 200 http://localhost:5176/api/content/posts => {"id":8}
- POST 200 http://localhost:5176/api/content/posts/image => {"url":"/uploads/posts/3baf6d9ecd23433e98ad7f45af1396fc.png"}
- GET 200 http://localhost:5176/api/content/posts?page=1&pageSize=10 => {"items":[{"id":8,"title":"Modal test Bài viết 1780409381111","slug":"Modal test Bài viết 1780409381111","category":"Modal test Bài viết 1780409381111","postStatus":"Draft","publishedAt":null,"createdDate":"2026-06-02T14:09:41.3802254"},{"id":7,"title":"Modal test Bài viết 1780408927713","slug":"Modal test Bài viết 1780408927713","category":"Modal

### 18. Bài viết - Sửa bài viết - Mở modal từ button-1

API responses:
- GET 200 http://localhost:5176/api/content/posts/8 => {"id":8,"title":"Modal test Bài viết 1780409381111","slug":"Modal test Bài viết 1780409381111","summary":"Modal test Bài viết 1780409381111","body":"Modal test Bài viết 1780409381111","coverUrl":"","category":"Modal test Bài viết 1780409381111","postStatus":"Draft","publishedAt":null}

### 19. Bài viết - Sửa bài viết - Generic submit

API responses:
- GET 200 http://localhost:5176/api/content/posts/8 => {"id":8,"title":"Modal test Bài viết 1780409381111","slug":"Modal test Bài viết 1780409381111","summary":"Modal test Bài viết 1780409381111","body":"Modal test Bài viết 1780409381111","coverUrl":"","category":"Modal test Bài viết 1780409381111","postStatus":"Draft","publishedAt":null}
- PUT 200 http://localhost:5176/api/content/posts/8 => {"id":8}
- POST 200 http://localhost:5176/api/content/posts/image => {"url":"/uploads/posts/932ab39b0bc240a9bfd8e46a2cebfc63.png"}
- GET 200 http://localhost:5176/api/content/posts?page=1&pageSize=10 => {"items":[{"id":8,"title":"Modal test Bài viết 1780409384960","slug":"Modal test Bài viết 1780409384960","category":"Modal test Bài viết 1780409384960","postStatus":"Draft","publishedAt":null,"createdDate":"2026-06-02T14:09:41.3802254"},{"id":7,"title":"Modal test Bài viết 1780408927713","slug":"Modal test Bài viết 1780408927713","category":"Modal

### 20. Bài viết - Sửa bài viết - Mở modal từ button-2

API responses:
- GET 200 http://localhost:5176/api/content/posts/7 => {"id":7,"title":"Modal test Bài viết 1780408927713","slug":"Modal test Bài viết 1780408927713","summary":"Modal test Bài viết 1780408927713","body":"Modal test Bài viết 1780408927713","coverUrl":"","category":"Modal test Bài viết 1780408927713","postStatus":"Draft","publishedAt":null}

### 21. Bài viết - Sửa bài viết - Generic submit

API responses:
- GET 200 http://localhost:5176/api/content/posts/7 => {"id":7,"title":"Modal test Bài viết 1780408927713","slug":"Modal test Bài viết 1780408927713","summary":"Modal test Bài viết 1780408927713","body":"Modal test Bài viết 1780408927713","coverUrl":"","category":"Modal test Bài viết 1780408927713","postStatus":"Draft","publishedAt":null}
- PUT 200 http://localhost:5176/api/content/posts/7 => {"id":7}
- POST 200 http://localhost:5176/api/content/posts/image => {"url":"/uploads/posts/e3dd4249a0a94aabbcfb125bb06a55ac.png"}
- GET 200 http://localhost:5176/api/content/posts?page=1&pageSize=10 => {"items":[{"id":8,"title":"Modal test Bài viết 1780409384960","slug":"Modal test Bài viết 1780409384960","category":"Modal test Bài viết 1780409384960","postStatus":"Draft","publishedAt":null,"createdDate":"2026-06-02T14:09:41.3802254"},{"id":7,"title":"Modal test Bài viết 1780409388779","slug":"Modal test Bài viết 1780409388779","category":"Modal

### 22. Banner - Thêm banner mới - Mở modal từ Thêm banner

- Không có dialog/API liên quan được capture.

### 23. Banner - Thêm banner mới - Generic submit

API responses:
- POST 200 http://localhost:5176/api/content/home-banners/image => {"url":"/uploads/banners/779677f8737440e3a0659fc60367f9e1.png"}
- POST 200 http://localhost:5176/api/content/home-banners => {"id":6}
- GET 200 http://localhost:5176/api/content/home-banners?all=true => {"items":[{"id":1,"position":"Slider","title":"Modal test Banner 1780408940147","imageUrl":"Modal test Banner 1780408940147","link":"Modal test Banner 1780408940147","sortOrder":1,"status":1},{"id":4,"position":"Slider","title":"Modal test Banner 1780408943862","imageUrl":"Modal test Banner 1780408943862","link":"Modal test Banner 1780408943862","s

### 24. Banner - Sửa banner - Mở modal từ button-1

- Không có dialog/API liên quan được capture.

### 25. Banner - Sửa banner - Generic submit

API responses:
- POST 200 http://localhost:5176/api/content/home-banners/image => {"url":"/uploads/banners/98df9366a24f4b9eaeedb5510c0df15c.png"}
- PUT 200 http://localhost:5176/api/content/home-banners/1 => {"id":1}
- GET 200 http://localhost:5176/api/content/home-banners?all=true => {"items":[{"id":1,"position":"Slider","title":"Modal test Banner 1780409397489","imageUrl":"Modal test Banner 1780409397489","link":"Modal test Banner 1780409397489","sortOrder":1,"status":1},{"id":4,"position":"Slider","title":"Modal test Banner 1780408943862","imageUrl":"Modal test Banner 1780408943862","link":"Modal test Banner 1780408943862","s

### 26. Banner - Sửa banner - Mở modal từ button-2

- Không có dialog/API liên quan được capture.

### 27. Banner - Sửa banner - Generic submit

API responses:
- POST 200 http://localhost:5176/api/content/home-banners/image => {"url":"/uploads/banners/edb60a71d50e4f52bd8c05fbc428a4d3.png"}
- PUT 200 http://localhost:5176/api/content/home-banners/4 => {"id":4}
- GET 200 http://localhost:5176/api/content/home-banners?all=true => {"items":[{"id":1,"position":"Slider","title":"Modal test Banner 1780409397489","imageUrl":"Modal test Banner 1780409397489","link":"Modal test Banner 1780409397489","sortOrder":1,"status":1},{"id":4,"position":"Slider","title":"Modal test Banner 1780409401436","imageUrl":"Modal test Banner 1780409401436","link":"Modal test Banner 1780409401436","s

### 28. Liên hệ - Chi tiết yêu cầu liên hệ - Mở modal từ button-0

- Không có dialog/API liên quan được capture.

### 29. Liên hệ - Chi tiết yêu cầu liên hệ - Mở modal từ button-1

- Không có dialog/API liên quan được capture.

### 30. Voucher - Thêm Voucher - Mở modal từ Thêm Voucher

- Không có dialog/API liên quan được capture.

### 31. Voucher - Thêm Voucher - Generic submit

API responses:
- POST 200 http://localhost:5176/api/vouchers => {"id":8}
- GET 200 http://localhost:5176/api/vouchers?page=1&pageSize=10 => {"items":[{"id":8,"code":"TST9411058","description":"Modal test Voucher 1780409411032","discountType":"Percent","discountValue":1.00,"maxDiscount":1.00,"minOrderValue":1.00,"usageLimit":1,"perUserLimit":null,"usedCount":0,"startAt":"2026-06-02T00:00:00","endAt":"2026-06-02T00:00:00","status":1},{"id":7,"code":"TST8956628","description":"Modal test

### 32. Voucher - Sửa Voucher - Mở modal từ button-1

- Không có dialog/API liên quan được capture.

### 33. Voucher - Sửa Voucher - Generic submit

API responses:
- PUT 200 http://localhost:5176/api/vouchers/8 => {"id":8}
- GET 200 http://localhost:5176/api/vouchers?page=1&pageSize=10 => {"items":[{"id":8,"code":"TST9415119","description":"Modal test Voucher 1780409415097","discountType":"Percent","discountValue":1.00,"maxDiscount":1.00,"minOrderValue":1.00,"usageLimit":1,"perUserLimit":null,"usedCount":0,"startAt":"2026-06-02T00:00:00","endAt":"2026-06-02T00:00:00","status":1},{"id":7,"code":"TST8956628","description":"Modal test

### 34. Voucher - Sửa Voucher - Mở modal từ button-2

- Không có dialog/API liên quan được capture.

### 35. Voucher - Sửa Voucher - Generic submit

API responses:
- PUT 200 http://localhost:5176/api/vouchers/7 => {"id":7}
- GET 200 http://localhost:5176/api/vouchers?page=1&pageSize=10 => {"items":[{"id":8,"code":"TST9415119","description":"Modal test Voucher 1780409415097","discountType":"Percent","discountValue":1.00,"maxDiscount":1.00,"minOrderValue":1.00,"usageLimit":1,"perUserLimit":null,"usedCount":0,"startAt":"2026-06-02T00:00:00","endAt":"2026-06-02T00:00:00","status":1},{"id":7,"code":"TST9419151","description":"Modal test

### 36. Xe máy - Thêm sản phẩm mới - Mở modal từ Thêm xe máy

- Không có dialog/API liên quan được capture.

### 37. Xe máy - Thêm sản phẩm mới - Generic submit

API responses:
- GET 200 http://localhost:5176/api/models?brandId=1 => {"items":[{"id":4,"brandId":1,"name":"Air Blade","slug":"honda-air-blade","status":1},{"id":12,"brandId":1,"name":"Modal Dòng xe 1780407665614","slug":"modal-dong-xe-1780407665614","status":1},{"id":13,"brandId":1,"name":"Modal Dòng xe 1780408142177","slug":"modal-dong-xe-1780408142177","status":1},{"id":14,"brandId":1,"name":"Modal Dòng xe 1780408
- POST 200 http://localhost:5176/api/products => {"id":2024}
- POST 200 http://localhost:5176/api/products/2024/images => {"id":15,"url":"/uploads/products/7d2e838af1454a8b8dea5a273a14c6b0.png"}
- GET 200 http://localhost:5176/api/products?page=1&pageSize=10&kind=1 => {"items":[{"id":2024,"code":"Modal test Xe máy 1780409423996","name":"Sản phẩm test 24066","slug":"Modal test Xe máy 1780409423996","categoryId":3,"brandId":1,"kind":1,"isFeatured":false,"isHotDeal":false,"listPrice":1.00,"salePrice":1.00,"mainImageUrl":"/uploads/products/7d2e838af1454a8b8dea5a273a14c6b0.png","manufacturerId":null,"manufacturerName

### 38. Xe máy - Sửa sản phẩm - Mở modal từ button-2

API responses:
- GET 200 http://localhost:5176/api/models?brandId=1 => {"items":[{"id":4,"brandId":1,"name":"Air Blade","slug":"honda-air-blade","status":1},{"id":12,"brandId":1,"name":"Modal Dòng xe 1780407665614","slug":"modal-dong-xe-1780407665614","status":1},{"id":13,"brandId":1,"name":"Modal Dòng xe 1780408142177","slug":"modal-dong-xe-1780408142177","status":1},{"id":14,"brandId":1,"name":"Modal Dòng xe 1780408

### 39. Xe máy - Sửa sản phẩm - Generic submit

API responses:
- GET 200 http://localhost:5176/api/models?brandId=1 => {"items":[{"id":4,"brandId":1,"name":"Air Blade","slug":"honda-air-blade","status":1},{"id":12,"brandId":1,"name":"Modal Dòng xe 1780407665614","slug":"modal-dong-xe-1780407665614","status":1},{"id":13,"brandId":1,"name":"Modal Dòng xe 1780408142177","slug":"modal-dong-xe-1780408142177","status":1},{"id":14,"brandId":1,"name":"Modal Dòng xe 1780408
- PUT 200 http://localhost:5176/api/products/2024 => {"id":2024}
- POST 200 http://localhost:5176/api/products/2024/images => {"id":16,"url":"/uploads/products/50a7032c49a94df3a29692997667ad17.png"}
- GET 200 http://localhost:5176/api/products?page=1&pageSize=10&kind=1 => {"items":[{"id":2024,"code":"Modal test Xe máy 1780409423996","name":"Sản phẩm test 46032","slug":"Modal test Xe máy 1780409445978","categoryId":3,"brandId":1,"kind":1,"isFeatured":false,"isHotDeal":false,"listPrice":1.00,"salePrice":1.00,"mainImageUrl":"/uploads/products/50a7032c49a94df3a29692997667ad17.png","manufacturerId":null,"manufacturerName

### 40. Phụ tùng - Thêm sản phẩm mới - Mở modal từ Thêm phụ tùng

- Không có dialog/API liên quan được capture.

### 41. Phụ tùng - Thêm sản phẩm mới - Generic submit

API responses:
- POST 200 http://localhost:5176/api/products => {"id":2025}
- POST 200 http://localhost:5176/api/products/2025/images => {"id":17,"url":"/uploads/products/bfc9582371804e29b177e86d134ea4ca.png"}
- GET 200 http://localhost:5176/api/products?page=1&pageSize=10&kind=2 => {"items":[{"id":2025,"code":"Modal test Phụ tùng 1780409466789","name":"Sản phẩm test 66854","slug":"Modal test Phụ tùng 1780409466789","categoryId":6,"brandId":null,"kind":2,"isFeatured":false,"isHotDeal":false,"listPrice":1.00,"salePrice":1.00,"mainImageUrl":"/uploads/products/bfc9582371804e29b177e86d134ea4ca.png","manufacturerId":null,"manufactu

### 42. Phụ tùng - Sửa sản phẩm - Mở modal từ button-2

- Không có dialog/API liên quan được capture.

### 43. Phụ tùng - Sửa sản phẩm - Generic submit

API responses:
- PUT 200 http://localhost:5176/api/products/2025 => {"id":2025}
- POST 200 http://localhost:5176/api/products/2025/images => {"id":18,"url":"/uploads/products/4163bec52fdc40a0ba03a5beb9e7ba07.png"}
- GET 200 http://localhost:5176/api/products?page=1&pageSize=10&kind=2 => {"items":[{"id":2025,"code":"Modal test Phụ tùng 1780409466789","name":"Sản phẩm test 88703","slug":"Modal test Phụ tùng 1780409488654","categoryId":6,"brandId":null,"kind":2,"isFeatured":false,"isHotDeal":false,"listPrice":1.00,"salePrice":1.00,"mainImageUrl":"/uploads/products/4163bec52fdc40a0ba03a5beb9e7ba07.png","manufacturerId":null,"manufactu

### 44. Tồn kho - Chi tiết giữ chỗ - Mở modal từ button-0

API responses:
- GET 200 http://localhost:5176/api/inventory/holds?productId=19&variantId=19 => {"items":[]}

### 45. Tồn kho - Cập nhật ngưỡng tồn thấp - Mở modal từ button-1

- Không có dialog/API liên quan được capture.

### 46. Tồn kho - Cập nhật ngưỡng tồn thấp - Generic submit

API responses:
- PUT 200 http://localhost:5176/api/inventory/threshold => {"message":"Cập nhật ngưỡng thành công."}
- GET 200 http://localhost:5176/api/inventory?page=1&pageSize=15&sortBy=product&sortDirection=asc => {"items":[{"storeId":1,"storeName":"Kho Online","skuId":19,"skuCode":"PT-ACQUY-GTZ5S","productName":"Ắc quy GS GTZ5S","onHand":17,"reserved":0,"available":17,"reorderPoint":1,"updatedAt":"2026-06-02T14:11:51.6698717"},{"storeId":2,"storeName":"Showroom HCM","skuId":19,"skuCode":"PT-ACQUY-GTZ5S","productName":"Ắc quy GS GTZ5S","onHand":15,"reserved"

### 47. Tồn kho - Nhập/Xuất/Điều chỉnh tồn - Mở modal từ button-2

- Không có dialog/API liên quan được capture.

### 48. Tồn kho - Nhập/Xuất/Điều chỉnh tồn - Generic submit

Dialogs:
- confirm: Xác nhận nhập kho? Tồn sau thay đổi: 18
API responses:
- POST 200 http://localhost:5176/api/inventory/adjust => {"message":"Điều chỉnh tồn thành công."}
- GET 200 http://localhost:5176/api/inventory?page=1&pageSize=15&sortBy=product&sortDirection=asc => {"items":[{"storeId":1,"storeName":"Kho Online","skuId":19,"skuCode":"PT-ACQUY-GTZ5S","productName":"Ắc quy GS GTZ5S","onHand":18,"reserved":0,"available":18,"reorderPoint":1,"updatedAt":"2026-06-02T14:11:55.5854876"},{"storeId":2,"storeName":"Showroom HCM","skuId":19,"skuCode":"PT-ACQUY-GTZ5S","productName":"Ắc quy GS GTZ5S","onHand":15,"reserved"
- GET 200 http://localhost:5176/api/inventory/adjustments => {"items":[{"id":1107,"storeId":1,"skuId":19,"type":1,"qtyDelta":1,"balanceAfter":18,"refType":"StockDocument","refId":0,"reason":"Modal test Tồn kho 1780409515354","occurredAt":"2026-06-02T14:11:56"},{"id":1106,"storeId":1,"skuId":19,"type":1,"qtyDelta":1,"balanceAfter":17,"refType":"StockDocument","refId":0,"reason":"Modal test Tồn kho 17804090568

### 49. Phiếu kho - Tạo phiếu kho - Mở modal từ Tạo phiếu kho

- Không có dialog/API liên quan được capture.

### 50. Phiếu kho - Tạo phiếu kho - Generic submit

API responses:
- POST 200 http://localhost:5176/api/inventory/documents => {"id":6}
- GET 200 http://localhost:5176/api/inventory/documents?pageSize=100 => {"items":[{"id":6,"code":"PK120260602141216","type":1,"status":"Draft","storeId":1,"storeName":"Kho Online","toStoreId":null,"toStoreName":null,"note":"Modal test Phiếu kho 1780409520063","createdDate":"2026-06-02T14:12:16.3061914","approvedAt":null,"lineCount":1},{"id":5,"code":"PK120260602140437","type":1,"status":"Draft","storeId":1,"storeName":

### 51. Phiếu kho - Chi tiết phiếu kho - PK120260602141216 - Mở modal từ button-1

API responses:
- GET 200 http://localhost:5176/api/inventory/documents/6 => {"document":{"id":6,"code":"PK120260602141216","type":1,"status":"Draft","storeId":1,"storeName":"Kho Online","toStoreId":null,"toStoreName":null,"note":"Modal test Phiếu kho 1780409520063","createdDate":"2026-06-02T14:12:16.3061914","approvedAt":null,"lineCount":1},"lines":[{"id":6,"skuId":12029,"skuCode":"Modal test Phụ tùng 1780407781570-DEFAULT

### 52. Phiếu kho - Chi tiết phiếu kho - PK120260602140437 - Mở modal từ button-2

API responses:
- GET 200 http://localhost:5176/api/inventory/documents/5 => {"document":{"id":5,"code":"PK120260602140437","type":1,"status":"Draft","storeId":1,"storeName":"Kho Online","toStoreId":null,"toStoreName":null,"note":"Modal test Phiếu kho 1780409061566","createdDate":"2026-06-02T14:04:37.8068251","approvedAt":null,"lineCount":1},"lines":[{"id":5,"skuId":12029,"skuCode":"Modal test Phụ tùng 1780407781570-DEFAULT

### 53. Khách hàng - Hồ sơ khách hàng 360 - Mở modal từ button-0

API responses:
- GET 200 http://localhost:5176/api/customers/13/profile => {"customer":{"id":13,"fullName":"Khach La","email":"lkh154036@test.local","phoneNumber":"0900000999","careNote":"Khách test 26872","createdDate":"2026-06-02T02:05:31","status":1},"summary":{"orderCount":0,"orderTotal":0,"remainingTotal":0,"warrantyCount":0,"repairCount":0,"openCrmCount":0},"orders":[],"warranties":[],"repairs":[],"interactions":[],

### 54. Khách hàng - Ghi chú chăm sóc - Khach La - Mở modal từ button-1

- Không có dialog/API liên quan được capture.

### 55. Khách hàng - Ghi chú chăm sóc - Khach La - Generic submit

API responses:
- PATCH 200 http://localhost:5176/api/users/customers/13/care-note => {"id":13}
- GET 200 http://localhost:5176/api/orders?page=1&pageSize=1000 => {"items":[{"id":6,"code":"DEMO-2026-006","orderStatus":"Cancelled","paymentStatus":"Unpaid","fulfillmentStatus":"Unallocated","grandTotal":450000.00,"placedAt":"2026-05-27T17:06:00","userId":3,"customerName":"Khách hàng mẫu","lines":[{"skuId":20,"productName":"Mũ bảo hiểm 3/4 MoToSale","skuCode":"PT-MBH-DEN-M","unitPrice":450000.00,"qty":1,"lineTot
- GET 200 http://localhost:5176/api/users/customers?pageSize=100 => {"items":[{"id":13,"fullName":"Khach La","email":"lkh154036@test.local","phoneNumber":"0900000999","status":1,"careNote":"Khách test 85340","createdDate":"2026-06-02T02:05:31"},{"id":11,"fullName":"Bùi Thành Công","email":"thanhcong@example.com","phoneNumber":"0955222333","status":0,"careNote":"Tài khoản khóa mẫu để kiểm tra bộ lọc trạng thái.","cr

### 56. Khách hàng - Hồ sơ khách hàng 360 - Mở modal từ button-2

API responses:
- GET 200 http://localhost:5176/api/customers/11/profile => {"customer":{"id":11,"fullName":"Bùi Thành Công","email":"thanhcong@example.com","phoneNumber":"0955222333","careNote":"Tài khoản khóa mẫu để kiểm tra bộ lọc trạng thái.","createdDate":"2026-06-02T00:44:24","status":0},"summary":{"orderCount":0,"orderTotal":0,"remainingTotal":0,"warrantyCount":0,"repairCount":0,"openCrmCount":0},"orders":[],"warran

### 57. Bảo hành - Tạo phiếu bảo hành - Mở modal từ Tạo phiếu bảo hành

- Không có dialog/API liên quan được capture.

### 58. Bảo hành - Tạo phiếu bảo hành - Generic submit

API responses:
- POST 200 http://localhost:5176/api/warranties => {"id":5}
- GET 200 http://localhost:5176/api/warranties?page=1&pageSize=100 => {"items":[{"id":5,"code":"BH20260602141336512","orderId":1,"skuId":1,"customerId":1,"productSnapshot":"Sản phẩm test 16121","serialNumber":"SKU616154","customerName":"Khách test 16066","customerPhone":"0901234567","frameNumber":"FRAME616293","engineNumber":"ENG616315","reportedIssue":"Khách báo lỗi kiểm thử","estimatedCost":1.00,"actualCost":null,"

### 59. Bảo hành - Chi tiết bảo hành - BH20260602141336512 - Mở modal từ button-1

API responses:
- GET 200 http://localhost:5176/api/warranties/5 => {"warranty":{"id":5,"code":"BH20260602141336512","orderId":1,"skuId":1,"customerId":1,"productSnapshot":"Sản phẩm test 16121","serialNumber":"SKU616154","customerName":"Khách test 16066","customerPhone":"0901234567","frameNumber":"FRAME616293","engineNumber":"ENG616315","reportedIssue":"Khách báo lỗi kiểm thử","estimatedCost":1.00,"actualCost":null

### 60. Bảo hành - Chi tiết bảo hành - BH20260602141336512 - Generic submit

API responses:
- GET 200 http://localhost:5176/api/warranties/5 => {"warranty":{"id":5,"code":"BH20260602141336512","orderId":1,"skuId":1,"customerId":1,"productSnapshot":"Sản phẩm test 16121","serialNumber":"SKU616154","customerName":"Khách test 16066","customerPhone":"0901234567","frameNumber":"FRAME616293","engineNumber":"ENG616315","reportedIssue":"Khách báo lỗi kiểm thử","estimatedCost":1.00,"actualCost":null
- PATCH 200 http://localhost:5176/api/warranties/5/status => {"id":5}
- GET 200 http://localhost:5176/api/warranties?page=1&pageSize=100 => {"items":[{"id":5,"code":"BH20260602141336512","orderId":1,"skuId":1,"customerId":1,"productSnapshot":"Sản phẩm test 16121","serialNumber":"SKU616154","customerName":"Khách test 16066","customerPhone":"0901234567","frameNumber":"FRAME616293","engineNumber":"ENG616315","reportedIssue":"Khách báo lỗi kiểm thử","estimatedCost":1.00,"actualCost":1.00,"
- GET 200 http://localhost:5176/api/warranties/5 => {"warranty":{"id":5,"code":"BH20260602141336512","orderId":1,"skuId":1,"customerId":1,"productSnapshot":"Sản phẩm test 16121","serialNumber":"SKU616154","customerName":"Khách test 16066","customerPhone":"0901234567","frameNumber":"FRAME616293","engineNumber":"ENG616315","reportedIssue":"Khách báo lỗi kiểm thử","estimatedCost":1.00,"actualCost":1.00

### 61. Bảo hành - Chi tiết bảo hành - BH20260602140558014 - Mở modal từ button-2

API responses:
- GET 200 http://localhost:5176/api/warranties/4 => {"warranty":{"id":4,"code":"BH20260602140558014","orderId":1,"skuId":1,"customerId":1,"productSnapshot":"Sản phẩm test 57622","serialNumber":"SKU157650","customerName":"Khách test 57567","customerPhone":"0901234567","frameNumber":"FRAME157774","engineNumber":"ENG157799","reportedIssue":"Khách báo lỗi kiểm thử","estimatedCost":1.00,"actualCost":1.00

### 62. Bảo hành - Chi tiết bảo hành - BH20260602140558014 - Generic submit

API responses:
- GET 200 http://localhost:5176/api/warranties/4 => {"warranty":{"id":4,"code":"BH20260602140558014","orderId":1,"skuId":1,"customerId":1,"productSnapshot":"Sản phẩm test 57622","serialNumber":"SKU157650","customerName":"Khách test 57567","customerPhone":"0901234567","frameNumber":"FRAME157774","engineNumber":"ENG157799","reportedIssue":"Khách báo lỗi kiểm thử","estimatedCost":1.00,"actualCost":1.00
- PATCH 200 http://localhost:5176/api/warranties/4/status => {"id":4}
- GET 200 http://localhost:5176/api/warranties?page=1&pageSize=100 => {"items":[{"id":5,"code":"BH20260602141336512","orderId":1,"skuId":1,"customerId":1,"productSnapshot":"Sản phẩm test 16121","serialNumber":"SKU616154","customerName":"Khách test 16066","customerPhone":"0901234567","frameNumber":"FRAME616293","engineNumber":"ENG616315","reportedIssue":"Khách báo lỗi kiểm thử","estimatedCost":1.00,"actualCost":1.00,"
- GET 200 http://localhost:5176/api/warranties/4 => {"warranty":{"id":4,"code":"BH20260602140558014","orderId":1,"skuId":1,"customerId":1,"productSnapshot":"Sản phẩm test 57622","serialNumber":"SKU157650","customerName":"Khách test 57567","customerPhone":"0901234567","frameNumber":"FRAME157774","engineNumber":"ENG157799","reportedIssue":"Khách báo lỗi kiểm thử","estimatedCost":1.00,"actualCost":1.00

### 63. Vận hành nâng cao - Tạo phiếu trả hàng - Mở modal từ Tạo phiếu trả hàng

- Không có dialog/API liên quan được capture.

### 64. Vận hành nâng cao - Tạo phiếu trả hàng - Generic submit

API responses:
- POST 200 http://localhost:5176/api/advanced-operations/returns => {"id":4}
- GET 200 http://localhost:5176/api/advanced-operations/returns => {"items":[{"id":4,"code":"RT20260602141420737","orderId":5,"orderCode":"DEMO-2026-005","storeId":1,"returnStatus":"Draft","reason":"Modal test Vận hành nâng cao 1780409660526","note":null,"refundAmount":0.00,"createdDate":"2026-06-02T14:14:20.7377208","approvedAt":null,"lines":[{"id":4,"orderLineId":6,"skuId":6,"productName":"Honda Wave Alpha","sku
- GET 200 http://localhost:5176/api/advanced-operations/refunds => {"items":[]}
- GET 200 http://localhost:5176/api/advanced-operations/shifts => {"items":[]}
- GET 200 http://localhost:5176/api/advanced-operations/receivables => {"items":[{"orderId":6,"orderCode":"DEMO-2026-006","customerName":"Khách hàng mẫu","grandTotal":450000.00,"depositRequired":0.00,"totalPaid":0,"totalRefunded":0,"netPaid":0,"outstanding":450000.00,"paymentStatus":"Unpaid"},{"orderId":5,"orderCode":"DEMO-2026-005","customerName":"Phạm Thu Trang","grandTotal":19145000.00,"depositRequired":0.00,"total
- GET 200 http://localhost:5176/api/business-operations/lookups => {"stores":[{"id":1,"code":"ONLINE","name":"Kho Online"},{"id":4,"code":"SR-DN","name":"Showroom Đà Nẵng"},{"id":3,"code":"SR-HN","name":"Showroom Hà Nội"},{"id":2,"code":"SR-HCM","name":"Showroom HCM"}],"skus":[{"id":12029,"skuCode":"Modal test Phụ tùng 1780407781570-DEFAULT","variantName":"Mặc định","productName":"Modal test Phụ tùng 1780407781570

### 65. Nghiệp vụ cửa hàng - Nhập thông tin nghiệp vụ - Mở modal từ Tạo mới

- Không có dialog/API liên quan được capture.

### 66. Nghiệp vụ cửa hàng - Nhập thông tin nghiệp vụ - Generic submit

API responses:
- POST 200 http://localhost:5176/api/business-operations/suppliers => {"id":5}
- GET 200 http://localhost:5176/api/business-operations/lookups => {"stores":[{"id":1,"code":"ONLINE","name":"Kho Online"},{"id":4,"code":"SR-DN","name":"Showroom Đà Nẵng"},{"id":3,"code":"SR-HN","name":"Showroom Hà Nội"},{"id":2,"code":"SR-HCM","name":"Showroom HCM"}],"skus":[{"id":12029,"skuCode":"Modal test Phụ tùng 1780407781570-DEFAULT","variantName":"Mặc định","productName":"Modal test Phụ tùng 1780407781570
- GET 200 http://localhost:5176/api/business-operations/summary => {"suppliers":1,"pendingPurchases":0,"purchaseValue":0.00,"cashIn":0.00,"cashOut":0.00,"openRepairs":0,"openInteractions":0}
- GET 200 http://localhost:5176/api/business-operations/suppliers => {"items":[{"code":"TST9667027","name":"Modal test Nghiệp vụ cửa hàng 1780409667009","taxCode":"TAX9667132","contactName":"Khách test 67084","phone":"0901234567","email":"","address":"","note":"","id":5,"createdDate":"2026-06-02T14:14:27.2186682","updatedDate":"2026-06-02T14:14:27.2186703","status":1}]}
- GET 200 http://localhost:5176/api/business-operations/repairs => {"items":[]}
- GET 200 http://localhost:5176/api/business-operations/cash => {"items":[]}
- GET 200 http://localhost:5176/api/business-operations/purchases => {"items":[]}
- GET 200 http://localhost:5176/api/business-operations/interactions => {"items":[]}
- GET 200 http://localhost:5176/api/business-operations/attendance => {"items":[]}
